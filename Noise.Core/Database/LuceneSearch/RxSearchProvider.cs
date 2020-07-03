using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database.LuceneSearch {
	public class SearchClient : ISearchClient {
		private readonly LuceneSearchProvider				mSearchProvider;
        private readonly SourceList<SearchResultItem>       mSearchSource;
        public	CancellationTokenSource						SearchCancellation;

        public IObservable<IChangeSet<SearchResultItem>>	SearchResults { get; }

		public SearchClient( LuceneSearchProvider searchProvider ) {
			mSearchProvider = searchProvider;

            mSearchSource = new SourceList<SearchResultItem>();
            SearchResults = mSearchSource.Connect();
        }

		public void ClearSearch() {
            SearchCancellation?.Cancel();
            SearchCancellation = null;

			mSearchSource.Clear();
        }

        public void StartSearch( eSearchItemType searchType, string queryText ) {
			ClearSearch();

            SearchCancellation = new CancellationTokenSource();

			mSearchProvider.StartSearch( this, searchType, queryText );
        }

        public void AddSearchResult( SearchResultItem item ) {
			mSearchSource.Add( item );
        }

        public void Dispose() {
            mSearchSource?.Dispose();
        }
    }

    partial class LuceneSearchProvider {
		private readonly int		mMaxResults = 1000;

		public void StartSearch( SearchClient searchClient, eSearchItemType searchType, string queryText ) {
            if(!mIsInitialized ) {
                Initialize();
            }

			Task.Run( () => {
				Search( searchClient, searchType, queryText  );
            });
        }

        public ISearchClient CreateSearchClient() {
			return new SearchClient( this );
        }

        private void Search( SearchClient searchClient, eSearchItemType searchType, string queryText ) {
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

					var	topDocs = searcher.Search( query, mMaxResults );
					var hits = topDocs.TotalHits;

					if(( hits > 0 ) &&
					   (!searchClient.SearchCancellation.IsCancellationRequested )) {
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

								if( searchClient.SearchCancellation.IsCancellationRequested ) {
									break;
                                }

								if(( artist != null ) ||
								   ( album != null ) ||
								   ( track != null )) {
									searchClient.AddSearchResult( new SearchResultItem( artist, album, track, itemType ));
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
        }
    }
}
