﻿using System;
using System.IO;
using DynamicData;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database.LuceneSearch {
    partial class LuceneSearchProvider : IRxSearchProvider, IDisposable {
		private readonly int										mMaxResults = 1000;
        private readonly SourceList<SearchResultItem>               mSearchSource;
        private readonly IObservable<IChangeSet<SearchResultItem>>  mSearchResults;

        public IObservableList<SearchResultItem>    SearchResults => mSearchResults.AsObservableList();

        public LuceneSearchProvider() {
            mSearchSource = new SourceList<SearchResultItem>();
            mSearchResults = mSearchSource.Connect();
        }

        public void Search( eSearchItemType searchType, string queryText ) {
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

					var	topDocs = searcher.Search( query, mMaxResults );
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
									mSearchSource.Add( new SearchResultItem( artist, album, track, itemType ) );
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

        public void Dispose() {
            mSearchSource?.Dispose();
        }
    }
}
