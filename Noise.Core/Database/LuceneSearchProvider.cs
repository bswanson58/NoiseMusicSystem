using System;
using System.Collections.Generic;
using System.IO;
using CuttingEdge.Conditions;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	internal class SearchItemFieldName {
		public	const string	cArtistId = "artistId";
		public	const string	cAlbumId = "albumId";
		public	const string	cTrackId = "trackId";
		public	const string	cItemType = "itemType";
		public	const string	cContent = "content";
		public	const string	cTimeStamp = "timeStamp";
	}

	internal class SearchItemDetails : IDisposable {
		private	IndexWriter		mWriter;
		private Document		mDocument;

		internal SearchItemDetails( IndexWriter writer, DbArtist artist, DateTime timeStamp ) {
			Condition.Requires( artist ).IsNotNull();

			mWriter = writer;
			mDocument = new Document();

			mDocument.Add( new Field( SearchItemFieldName.cItemType, eSearchItemType.TimeStamp.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED ));
			mDocument.Add( new Field( SearchItemFieldName.cArtistId, artist.DbId.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED ));
			mDocument.Add( new Field( SearchItemFieldName.cTimeStamp, timeStamp.Ticks.ToString(), Field.Store.YES, Field.Index.NO ));
		}

		internal SearchItemDetails( IndexWriter writer, eSearchItemType itemType, DbArtist artist ) :
			this( writer, itemType, artist, null, null ) {
		}

		internal SearchItemDetails( IndexWriter writer, eSearchItemType itemType, DbArtist artist, DbAlbum album ) :
			this( writer, itemType, artist, album, null ) {
		}

		internal SearchItemDetails( IndexWriter writer, eSearchItemType itemType, DbArtist artist, DbAlbum album, DbTrack track ) {
			Condition.Requires( artist ).IsNotNull();

			mWriter = writer;
			mDocument = new Document();

			mDocument.Add( new Field( SearchItemFieldName.cItemType, itemType.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED ));
			mDocument.Add( new Field( SearchItemFieldName.cArtistId, artist.DbId.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED ));
			if( album != null ) {
				mDocument.Add( new Field( SearchItemFieldName.cAlbumId, album.DbId.ToString(), Field.Store.YES, Field.Index.NO ));
			}
			if( track != null ) {
				mDocument.Add( new Field( SearchItemFieldName.cTrackId, track.DbId.ToString(), Field.Store.YES, Field.Index.NO ));
			}

			var	boost = 1.0f;
			switch( itemType ) {
				case eSearchItemType.Album:
					boost = 1.3f;
					break;

				case eSearchItemType.Artist:
					boost = 1.2f;
					break;

				case eSearchItemType.Track:
					boost = 1.1f;
					break;

				case eSearchItemType.BandMember:
				case eSearchItemType.Biography:
				case eSearchItemType.TextInfo:
				case eSearchItemType.TopAlbum:
				case eSearchItemType.TimeStamp:
					break;

				case eSearchItemType.Discography:
				case eSearchItemType.SimilarArtist:
					boost = 0.8f;
					break;
			}

			mDocument.SetBoost( boost );
		}

		public void AddSearchText( string searchText ) {
			Condition.Requires( mDocument ).IsNotNull();

			mDocument.Add( new Field( SearchItemFieldName.cContent, searchText, Field.Store.NO, Field.Index.ANALYZED ));
		}

		public void AddSearchText( IEnumerable<string> searchText ) {
			foreach( var text in searchText ) {
				AddSearchText( text );
			}
		}

		public void CommitItem() {
			Condition.Requires( mDocument ).IsNotNull();

			if(( mDocument != null ) &&
			   ( mWriter != null )) {
				mWriter.AddDocument( mDocument );
				
				mDocument = null;
				mWriter = null;
			}
		}

		public void Dispose() {
			CommitItem();
		}
	}

	public class LuceneSearchProvider : ISearchProvider {
		private readonly IUnityContainer	mContainer;
		private readonly ILog				mLog;
		private bool						mIsInitialized;
		private	string						mIndexLocation;
		private IndexWriter					mIndexWriter;

		public LuceneSearchProvider( IUnityContainer container ) {
			mContainer = container;
			mLog = mContainer.Resolve<ILog>();
		}

		public bool Initialize() {
			mIsInitialized = false;

			var configMgr = mContainer.Resolve<ISystemConfiguration>();
			var databaseManager = mContainer.Resolve<IDatabaseManager>();
			var	database = databaseManager.ReserveDatabase();

			try {
				var config = configMgr.RetrieveConfiguration<DatabaseConfiguration>( DatabaseConfiguration.SectionName );

				if( config != null ) {
					mIndexLocation = config.SearchIndexLocation;

					var index = mIndexLocation.ToUpper().IndexOf( "%APPDATA%", StringComparison.OrdinalIgnoreCase );
					if( index != -1 ) {
						mIndexLocation = mIndexLocation.Remove( index, "%APPDATA%".Length );
						mIndexLocation = mIndexLocation.Insert( index, Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), 
																					 Constants.CompanyName ));
						if(!Directory.Exists( mIndexLocation )) {
							Directory.CreateDirectory( mIndexLocation );
						}

						mIndexLocation = Path.Combine( mIndexLocation, database.DatabaseVersion.DatabaseId.ToString());
						if(!Directory.Exists( mIndexLocation )) {
							Directory.CreateDirectory( mIndexLocation );
						}

						var directory = new Lucene.Net.Store.SimpleFSDirectory( new DirectoryInfo( mIndexLocation ));
						if(!IndexReader.IndexExists( directory )) {
							var analyzer = new Lucene.Net.Analysis.Standard.StandardAnalyzer( Lucene.Net.Util.Version.LUCENE_29 );
							var indexWriter = new IndexWriter( directory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED );

							indexWriter.Close();
						}
					}

					if( Directory.Exists( mIndexLocation )) {
						mIsInitialized = true;
					}
				}
				else {
					mLog.LogMessage( "Database configuration could not be loaded." );
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - Initializing the Lucene Search Provider", ex );
			}
			finally {
				databaseManager.FreeDatabase( database );
			}

			return( mIsInitialized );
		}

		public IEnumerable<SearchResultItem> Search( string queryText, int maxResults ) {
			var	retValue = new List<SearchResultItem>();

			if(!mIsInitialized ) {
				Initialize();
			}

			if( mIsInitialized ) {
				try {
					var noiseManager = mContainer.Resolve<INoiseManager>();
					var directory = new Lucene.Net.Store.SimpleFSDirectory( new DirectoryInfo( mIndexLocation ));
					var	searcher = new IndexSearcher( directory, true );
					var queryParser = new QueryParser( Lucene.Net.Util.Version.LUCENE_29, SearchItemFieldName.cContent, 
														new Lucene.Net.Analysis.Standard.StandardAnalyzer( Lucene.Net.Util.Version.LUCENE_29 ));
					var query = queryParser.Parse( queryText );

					var	topDocs = searcher.Search( query, maxResults );
					var hits = topDocs.totalHits;

					if( hits > 0 ) {
						foreach( var hit in topDocs.scoreDocs ) {
							var document = searcher.Doc( hit.doc );

							var typeField = document.GetField( SearchItemFieldName.cItemType );
							var	itemType = eSearchItemType.Unknown;
							if( typeField != null ) {
								itemType = (eSearchItemType)Enum.Parse( typeof( eSearchItemType ), typeField.StringValue());
							}

							if(( itemType != eSearchItemType.TimeStamp ) &&
							   ( itemType != eSearchItemType.Unknown )) {
								var artistField = document.GetField( SearchItemFieldName.cArtistId );
								var albumField = document.GetField( SearchItemFieldName.cAlbumId );
								var trackField = document.GetField( SearchItemFieldName.cTrackId );

								DbArtist		artist = null;
								DbAlbum			album = null;
								DbTrack			track = null;

								if( artistField != null ) {
									long	id = long.Parse( artistField.StringValue());

									artist = noiseManager.DataProvider.GetArtist( id );
								}
								if( albumField != null ) {
									long	id = long.Parse( albumField.StringValue());

									album = noiseManager.DataProvider.GetAlbum( id );
								}
								if( trackField != null ) {
									long	id = long.Parse( trackField.StringValue());

									track = noiseManager.DataProvider.GetTrack( id );
								}

								retValue.Add( new SearchResultItem( artist, album, track, itemType ));
							}
						}
					}

					searcher.Close();
				}
				catch( Exception ex ) {
					mLog.LogException( "Exception - Search failed: ", ex );
				}
			}

			return( retValue );
		}

		public DateTime DetermineTimeStamp( DbArtist forArtist ) {
			var	retValue = new DateTime();

			if(!mIsInitialized ) {
				Initialize();
			}

			if( mIsInitialized ) {
				try {
					var directory = new Lucene.Net.Store.SimpleFSDirectory( new DirectoryInfo( mIndexLocation ));
					var	searcher = new IndexSearcher( directory, true );
					var artistTerm = new TermQuery( new Term( SearchItemFieldName.cArtistId, forArtist.DbId.ToString()));
					var typeTerm = new TermQuery( new Term( SearchItemFieldName.cItemType, eSearchItemType.TimeStamp.ToString()));
					var query = new BooleanQuery();

					query.Add( artistTerm, BooleanClause.Occur.MUST );
					query.Add( typeTerm, BooleanClause.Occur.MUST );

					var	topDocs = searcher.Search( query, 1 );
					var hits = topDocs.totalHits;

					if( hits > 0 ) {
						foreach( var hit in topDocs.scoreDocs ) {
							var document = searcher.Doc( hit.doc );
							var artistField = document.GetField( SearchItemFieldName.cArtistId );
							var timeField = document.GetField( SearchItemFieldName.cTimeStamp );

							if(( artistField != null ) &&
							   ( timeField != null )) {
								long	id = long.Parse( artistField.StringValue());
								long	time = long.Parse( timeField.StringValue());

								if( id == forArtist.DbId ) {
									retValue = new DateTime( time );

									break;
								}
							}
						}
					}

					searcher.Close();
				}
				catch( Exception ex ) {
					mLog.LogException( "Exception - DetermineTimeStamp: ", ex );
				}
			}
			return( retValue );
		}

		public bool StartIndexUpdate( bool createIndex ) {
			var retValue = false;

			if(!mIsInitialized ) {
				Initialize();
			}

			if( mIsInitialized ) {
				try {
					var directory = new Lucene.Net.Store.SimpleFSDirectory( new DirectoryInfo( mIndexLocation ));
					var analyzer = new Lucene.Net.Analysis.Standard.StandardAnalyzer( Lucene.Net.Util.Version.LUCENE_29 );

					mIndexWriter = new IndexWriter( directory, analyzer, createIndex, IndexWriter.MaxFieldLength.UNLIMITED );

					retValue = true;
				}
				catch( Exception ex ) {
					mLog.LogException( "Exception - Search provider cannot open IndexWriter: ", ex );
				}
			}

			return( retValue );
		}

		public void WriteTimeStamp( DbArtist artist ) {
			using( new SearchItemDetails( mIndexWriter, artist, DateTime.Now )) { }
		}

		private SearchItemDetails AddSearchItem( DbArtist artist, eSearchItemType itemType ) {
			Condition.Requires( mIndexWriter ).IsNotNull();
			Condition.Requires( artist ).IsNotNull();

			return( new SearchItemDetails( mIndexWriter, itemType, artist ));
		}

		public void AddSearchItem( DbArtist artist, eSearchItemType itemType, string searchText ) {
			Condition.Requires( mIndexWriter ).IsNotNull();
			Condition.Requires( artist ).IsNotNull();

			using( var searchItem = AddSearchItem( artist, itemType )) {
				searchItem.AddSearchText( searchText );
			}
		}

		public void AddSearchItem( DbArtist artist, eSearchItemType itemType, IEnumerable<string> searchList ) {
			Condition.Requires( mIndexWriter ).IsNotNull();
			Condition.Requires( artist ).IsNotNull();

			foreach( var searchText in searchList ) {
				using( var searchItem = AddSearchItem( artist, itemType )) {
					searchItem.AddSearchText( searchText );
				}
			}
		}

		private SearchItemDetails AddSearchItem( DbArtist artist, DbAlbum album, eSearchItemType itemType ) {
			Condition.Requires( mIndexWriter ).IsNotNull();
			Condition.Requires( artist ).IsNotNull();
			Condition.Requires( album ).IsNotNull();

			return( new SearchItemDetails( mIndexWriter, itemType, artist, album ));
		}

		public void AddSearchItem( DbArtist artist, DbAlbum album, eSearchItemType itemType, string searchText ) {
			Condition.Requires( mIndexWriter ).IsNotNull();
			Condition.Requires( artist ).IsNotNull();
			Condition.Requires( album ).IsNotNull();

			using( var searchItem = AddSearchItem( artist, album, itemType )) {
				searchItem.AddSearchText( searchText );
			}
		}

		private SearchItemDetails AddSearchItem( DbArtist artist, DbAlbum album, DbTrack track, eSearchItemType itemType ) {
			Condition.Requires( mIndexWriter ).IsNotNull();
			Condition.Requires( artist ).IsNotNull();
			Condition.Requires( album ).IsNotNull();
			Condition.Requires( track ).IsNotNull();

			return( new SearchItemDetails( mIndexWriter, itemType, artist, album, track ));
		}

		public void AddSearchItem( DbArtist artist, DbAlbum album, DbTrack track, eSearchItemType itemType, string searchText ) {
			Condition.Requires( mIndexWriter ).IsNotNull();
			Condition.Requires( artist ).IsNotNull();
			Condition.Requires( album ).IsNotNull();
			Condition.Requires( track ).IsNotNull();

			using( var searchItem = AddSearchItem( artist, album, track, itemType )) {
				searchItem.AddSearchText( searchText );
			}
		}

		public void DeleteArtistSearchItems( DbArtist forArtist ) {
			mIndexWriter.DeleteDocuments( new Term( SearchItemFieldName.cArtistId, forArtist.DbId.ToString()));
		}

		public bool EndIndexUpdate() {
			var retValue = false;

			if( mIndexWriter != null ) {
				try {
					mIndexWriter.Optimize();
					mIndexWriter.Close();

					retValue = true;
				}
				catch( Exception ex ) {
					mLog.LogException( "Exception - Search provider cannot close IndexWriter: ", ex );
				}
			}

			return( retValue );
		}
	}
}
