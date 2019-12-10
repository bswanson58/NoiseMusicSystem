using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database.LuceneSearch {
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
                        var boolQuery = new BooleanQuery { { textQuery, Occur.MUST }, { typeTerm, Occur.MUST } };

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
                    var query = new BooleanQuery { { artistTerm, Occur.MUST }, { typeTerm, Occur.MUST } };
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
					mLog.LogException( $"Cannot determine timestamp for {forArtist}", ex );
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
					mLog.LogException( $"Cannot create index builder for {artist}", ex );
				}
			}

			return( retValue );
		}
	}
}
