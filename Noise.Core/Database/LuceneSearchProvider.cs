using System;
using System.Collections.Generic;
using CuttingEdge.Conditions;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	internal class SearchItemFieldName {
		public	const string	cArtistId = "artistId";
		public	const string	cAlbumId = "albumId";
		public	const string	cItemType = "itemType";
		public	const string	cContent = "content";
	}

	internal class SearchItemDetails : IDisposable {
		private	IndexWriter		mWriter;
		private Document		mDocument;

		internal SearchItemDetails( IndexWriter writer, eSearchItemType itemType, DbArtist artist, DbAlbum album ) {
			Condition.Requires( artist ).IsNotNull();

			mWriter = writer;
			mDocument = new Document();

			mDocument.Add( new Field( SearchItemFieldName.cItemType, itemType.ToString(), Field.Store.YES, Field.Index.NO ));
			mDocument.Add( new Field( SearchItemFieldName.cArtistId, artist.DbId.ToString(), Field.Store.YES, Field.Index.NO ));
			if( album != null ) {
				mDocument.Add( new Field( SearchItemFieldName.cAlbumId, album.DbId.ToString(), Field.Store.YES, Field.Index.NO ));
			}
		}

		internal SearchItemDetails( IndexWriter writer, eSearchItemType itemType, DbArtist artist ) :
			this( writer, itemType, artist, null ) {
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
			var config = configMgr.RetrieveConfiguration<DatabaseConfiguration>( DatabaseConfiguration.SectionName );

			if( config != null ) {
				mIndexLocation = config.SearchIndexLocation;

				if( System.IO.Directory.Exists( mIndexLocation )) {
					mIsInitialized = true;
				}
			}
			else {
				mLog.LogMessage( "Database configuration could not be loaded." );
			}

			return( mIsInitialized );
		}

		public IEnumerable<SearchResultItem> Search( string queryText ) {
			var	retValue = new List<SearchResultItem>();

			if(!mIsInitialized ) {
				Initialize();
			}

			if( mIsInitialized ) {
				try {
					var noiseManager = mContainer.Resolve<INoiseManager>();
					var directory = new Lucene.Net.Store.SimpleFSDirectory( new System.IO.DirectoryInfo( mIndexLocation ));
					var	searcher = new IndexSearcher( directory, true );
					var queryParser = new QueryParser( Lucene.Net.Util.Version.LUCENE_29, "content", 
														new Lucene.Net.Analysis.Standard.StandardAnalyzer( Lucene.Net.Util.Version.LUCENE_29 ));
					var query = queryParser.Parse( queryText );

					var	topDocs = searcher.Search( query, 10 );
					var hits = topDocs.totalHits;

					if( hits > 0 ) {
						foreach( var hit in topDocs.scoreDocs ) {
							var document = searcher.Doc( hit.doc );

							var typeField = document.GetField( SearchItemFieldName.cItemType );
							var artistField = document.GetField( SearchItemFieldName.cArtistId );
							var albumField = document.GetField( SearchItemFieldName.cAlbumId );

							DbArtist		artist = null;
							DbAlbum			album = null;
							eSearchItemType	itemType = eSearchItemType.Unknown;

							if( artistField != null ) {
								long	id = long.Parse( artistField.StringValue());

								artist = noiseManager.DataProvider.GetArtist( id );
							}
							if( albumField != null ) {
								long	id = long.Parse( albumField.StringValue());

								album = noiseManager.DataProvider.GetAlbum( id );
							}
							if( typeField != null ) {
								itemType = (eSearchItemType)Enum.Parse( typeof( eSearchItemType ), typeField.StringValue());
							}

							retValue.Add( new SearchResultItem( artist, album, itemType ));
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

		public bool StartIndexUpdate( bool createIndex ) {
			var retValue = false;

			if(!mIsInitialized ) {
				Initialize();
			}

			if( mIsInitialized ) {
				try {
					var directory = new Lucene.Net.Store.SimpleFSDirectory( new System.IO.DirectoryInfo( mIndexLocation ));
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
