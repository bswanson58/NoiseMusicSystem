using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CuttingEdge.Conditions;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
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

		internal SearchItemDetails( IndexWriter writer, long artistId, DateTime timeStamp ) {
			mWriter = writer;
			mDocument = new Document();

			mDocument.Add( new Field( SearchItemFieldName.cItemType, eSearchItemType.TimeStamp.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED ));
			mDocument.Add( new Field( SearchItemFieldName.cArtistId, artistId.ToString( CultureInfo.InvariantCulture ), Field.Store.YES, Field.Index.NOT_ANALYZED ));
			mDocument.Add( new Field( SearchItemFieldName.cTimeStamp, timeStamp.Ticks.ToString( CultureInfo.InvariantCulture ), Field.Store.YES, Field.Index.NO ));
		}

		internal SearchItemDetails( IndexWriter writer, eSearchItemType itemType, long artistId ) :
			this( writer, itemType, artistId, null, null ) {
		}

		internal SearchItemDetails( IndexWriter writer, eSearchItemType itemType, long artistId, DbAlbum album ) :
			this( writer, itemType, artistId, album, null ) {
		}

		internal SearchItemDetails( IndexWriter writer, eSearchItemType itemType, long artistId, DbAlbum album, DbTrack track ) {
			mWriter = writer;
			mDocument = new Document();

			mDocument.Add( new Field( SearchItemFieldName.cItemType, itemType.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED ));
			mDocument.Add( new Field( SearchItemFieldName.cArtistId, artistId.ToString( CultureInfo.InvariantCulture ), Field.Store.YES, Field.Index.NOT_ANALYZED ));
			if( album != null ) {
				mDocument.Add( new Field( SearchItemFieldName.cAlbumId, album.DbId.ToString( CultureInfo.InvariantCulture ), Field.Store.YES, Field.Index.NO ));
			}
			if( track != null ) {
				mDocument.Add( new Field( SearchItemFieldName.cTrackId, track.DbId.ToString( CultureInfo.InvariantCulture ), Field.Store.YES, Field.Index.NO ));
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
				case eSearchItemType.Lyrics:
				case eSearchItemType.TextInfo:
				case eSearchItemType.TopAlbum:
				case eSearchItemType.TimeStamp:
					break;

				case eSearchItemType.Discography:
				case eSearchItemType.SimilarArtist:
					boost = 0.8f;
					break;
			}

			mDocument.Boost = boost;
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

	public class LuceneIndexBuilder : ISearchBuilder {
		private readonly INoiseLog	mLog;
		private readonly long		mArtistId;
		private IndexWriter			mIndexWriter;

		public LuceneIndexBuilder( DbArtist artist, string indexLocation, bool createIndex, INoiseLog log ) {
			mLog = log;
			mArtistId = artist.DbId;

			var directory = new Lucene.Net.Store.SimpleFSDirectory( new DirectoryInfo( indexLocation ));
			var analyzer = new Lucene.Net.Analysis.Standard.StandardAnalyzer( Lucene.Net.Util.Version.LUCENE_29 );

			mIndexWriter = new IndexWriter( directory, analyzer, createIndex, IndexWriter.MaxFieldLength.UNLIMITED );
		}

		public void WriteTimeStamp() {
			using( new SearchItemDetails( mIndexWriter, mArtistId, DateTime.Now )) { }
		}

		private SearchItemDetails AddSearchItem( eSearchItemType itemType ) {
			return( new SearchItemDetails( mIndexWriter, itemType, mArtistId ));
		}

		public void AddSearchItem( eSearchItemType itemType, string searchText ) {
			using( var searchItem = AddSearchItem( itemType )) {
				searchItem.AddSearchText( searchText );
			}
		}

		public void AddSearchItem( eSearchItemType itemType, IEnumerable<string> searchList ) {
			foreach( var searchText in searchList ) {
				using( var searchItem = AddSearchItem( itemType )) {
					searchItem.AddSearchText( searchText );
				}
			}
		}

		private SearchItemDetails AddSearchItem( DbAlbum album, eSearchItemType itemType ) {
			Condition.Requires( album ).IsNotNull();

			return( new SearchItemDetails( mIndexWriter, itemType, mArtistId, album ));
		}

		public void AddSearchItem( DbAlbum album, eSearchItemType itemType, string searchText ) {
			Condition.Requires( album ).IsNotNull();

			using( var searchItem = AddSearchItem( album, itemType )) {
				searchItem.AddSearchText( searchText );
			}
		}

		private SearchItemDetails AddSearchItem( DbAlbum album, DbTrack track, eSearchItemType itemType ) {
			Condition.Requires( album ).IsNotNull();
			Condition.Requires( track ).IsNotNull();

			return( new SearchItemDetails( mIndexWriter, itemType, mArtistId, album, track ));
		}

		public void AddSearchItem( DbAlbum album, DbTrack track, eSearchItemType itemType, string searchText ) {
			Condition.Requires( album ).IsNotNull();
			Condition.Requires( track ).IsNotNull();

			using( var searchItem = AddSearchItem( album, track, itemType )) {
				searchItem.AddSearchText( searchText );
			}
		}

		public void DeleteArtistSearchItems() {
			mIndexWriter.DeleteDocuments( new Term( SearchItemFieldName.cArtistId, mArtistId.ToString( CultureInfo.InvariantCulture )));
		}

		public void Dispose() {
			if( mIndexWriter != null ) {
				EndIndexUpdate();
			}
		}

		private void EndIndexUpdate() {
			if( mIndexWriter != null ) {
				try {
					mIndexWriter.Optimize();
					mIndexWriter.Dispose();
					mIndexWriter = null;
				}
				catch( Exception ex ) {
					mLog.LogException( "Search provider cannot close IndexWriter", ex );
				}
			}
		}
	}

	public class LuceneSearchProvider : ISearchProvider {
		private readonly ILibraryConfiguration	mLibraryConfiguration;
		private readonly IArtistProvider		mArtistProvider;
		private readonly IAlbumProvider			mAlbumProvider;
		private readonly ITrackProvider			mTrackProvider;
		private readonly INoiseLog				mLog;
		private bool							mIsInitialized;
		private	string							mIndexLocation;

		public LuceneSearchProvider( ILibraryConfiguration libraryConfiguration,
									 IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider, INoiseLog log ) {
			mLibraryConfiguration = libraryConfiguration;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mLog = log;
		}

		public bool Initialize() {
			mIsInitialized = false;

			try {
				if( mLibraryConfiguration.Current != null ) {
					mIndexLocation = mLibraryConfiguration.Current.SearchDatabasePath;

					var directory = new Lucene.Net.Store.SimpleFSDirectory( new DirectoryInfo( mIndexLocation ));
					if(!IndexReader.IndexExists( directory )) {
						var analyzer = new Lucene.Net.Analysis.Standard.StandardAnalyzer( Lucene.Net.Util.Version.LUCENE_29 );
						var indexWriter = new IndexWriter( directory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED );

						indexWriter.Dispose();
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
				mLog.LogException( "Initializing the Lucene Search Provider", ex );
			}

			return( mIsInitialized );
		}

		public IEnumerable<SearchResultItem> Search( eSearchItemType searchType, string queryText, int maxResults ) {
			var	retValue = new List<SearchResultItem>();

			if(!mIsInitialized ) {
				Initialize();
			}

			if( mIsInitialized ) {
				try {
//					var noiseManager = mContainer.Resolve<INoiseManager>();
					var directory = new Lucene.Net.Store.SimpleFSDirectory( new DirectoryInfo( mIndexLocation ));
					var	searcher = new IndexSearcher( directory, true );
					var queryParser = new QueryParser( Lucene.Net.Util.Version.LUCENE_29, SearchItemFieldName.cContent, 
														new Lucene.Net.Analysis.Standard.StandardAnalyzer( Lucene.Net.Util.Version.LUCENE_29 ));
					var textQuery = queryParser.Parse( queryText );
					Query	query;

					if( searchType == eSearchItemType.Everything ) {
						query = textQuery;
					}
					else {
						var typeTerm = new TermQuery( new Term( SearchItemFieldName.cItemType, searchType.ToString()));
						var	boolQuery = new BooleanQuery();

						boolQuery.Add( textQuery, Occur.MUST );
						boolQuery.Add( typeTerm, Occur.MUST );

						query = boolQuery;
					}

					var	topDocs = searcher.Search( query, maxResults );
					var hits = topDocs.TotalHits;

					if( hits > 0 ) {
						foreach( var hit in topDocs.ScoreDocs ) {
							var document = searcher.Doc( hit.Doc );

							var typeField = document.GetField( SearchItemFieldName.cItemType );
							var	itemType = eSearchItemType.Unknown;
							if( typeField != null ) {
								itemType = (eSearchItemType)Enum.Parse( typeof( eSearchItemType ), typeField.StringValue );
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
									long	id = long.Parse( artistField.StringValue );

									artist = mArtistProvider.GetArtist( id );
								}
								if( albumField != null ) {
									long	id = long.Parse( albumField.StringValue );

									album = mAlbumProvider.GetAlbum( id );
								}
								if( trackField != null ) {
									long	id = long.Parse( trackField.StringValue );

									track = mTrackProvider.GetTrack( id );
								}

								if(( artist != null ) ||
								   ( album != null ) ||
								   ( track != null )) {
									retValue.Add( new SearchResultItem( artist, album, track, itemType ) );
								}
							}
						}
					}

					searcher.Dispose();
				}
				catch( Exception ex ) {
					mLog.LogException( "Search failed", ex );
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
					var artistTerm = new TermQuery( new Term( SearchItemFieldName.cArtistId, forArtist.DbId.ToString( CultureInfo.InvariantCulture )));
					var typeTerm = new TermQuery( new Term( SearchItemFieldName.cItemType, eSearchItemType.TimeStamp.ToString()));
					var query = new BooleanQuery();

					query.Add( artistTerm, Occur.MUST );
					query.Add( typeTerm, Occur.MUST );

					var	topDocs = searcher.Search( query, 1 );
					var hits = topDocs.TotalHits;

					if( hits > 0 ) {
						foreach( var hit in topDocs.ScoreDocs ) {
							var document = searcher.Doc( hit.Doc );
							var artistField = document.GetField( SearchItemFieldName.cArtistId );
							var timeField = document.GetField( SearchItemFieldName.cTimeStamp );

							if(( artistField != null ) &&
							   ( timeField != null )) {
								long	id = long.Parse( artistField.StringValue );
								long	time = long.Parse( timeField.StringValue );

								if( id == forArtist.DbId ) {
									retValue = new DateTime( time );

									break;
								}
							}
						}
					}

					searcher.Dispose();
				}
				catch( Exception ex ) {
					mLog.LogException( string.Format( "Cannot determine timestamp for {0}", forArtist ), ex );
				}
			}
			return( retValue );
		}

		public ISearchBuilder CreateIndexBuilder( DbArtist artist, bool createIndex ) {
			ISearchBuilder	retValue = null;

			if(!mIsInitialized ) {
				Initialize();
			}

			if( mIsInitialized ) {
				try {
					retValue = new LuceneIndexBuilder( artist, mIndexLocation, createIndex, mLog );
				}
				catch( Exception ex ) {
					mLog.LogException( string.Format( "Cannot create index builder for {0}", artist ), ex );
				}
			}

			return( retValue );
		}
	}
}
