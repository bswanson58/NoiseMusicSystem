using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteServer.Services {
    class SearchService : SearchInformation.SearchInformationBase  {
        private readonly ISearchProvider    mSearchProvider;
        private readonly INoiseLog          mLog;

        public SearchService( ISearchProvider searchProvider, INoiseLog log ) {
            mSearchProvider = searchProvider;
            mLog = log;
        }

        private SearchItemInfo TransformSearchResult( SearchResultItem searchItem ) {
            var retValue = new SearchItemInfo();

            if( searchItem.Artist != null ) {
                retValue.ArtistId = searchItem.Artist.DbId;
                retValue.ArtistName = searchItem.Artist.Name;
            }

            if( searchItem.Album != null ) {
                retValue.AlbumId = searchItem.Album.DbId;
                retValue.AlbumName = searchItem.Album.Name;
            }

            if( searchItem.Track != null ) {
                retValue.TrackId = searchItem.Track.DbId;
                retValue.TrackName = searchItem.Track.Name;
                retValue.VolumeName = searchItem.Track.VolumeName;
                retValue.TrackNumber = searchItem.Track.TrackNumber;
                retValue.Duration = searchItem.Track.DurationMilliseconds;
                retValue.IsFavorite = searchItem.Track.IsFavorite;
                retValue.Rating = searchItem.Track.Rating;
            }

            return retValue;
        }

        public override Task<SearchResponse> Search( SearchRequest request, ServerCallContext context ) {
            return Task.Run( () => {
                var retValue = new SearchResponse();

                try {
//                    var artistResults = mSearchProvider.Search( eSearchItemType.Artist, request.SearchTerm, 100 );
//                    var albumResults = mSearchProvider.Search( eSearchItemType.Album, request.SearchTerm, 100 );
                    var trackResults = mSearchProvider.Search( eSearchItemType.Track, request.SearchTerm, 1000 );

//                    retValue.SearchResults.AddRange( from a in artistResults select TransformSearchResult( a ));
//                    retValue.SearchResults.AddRange( from a in albumResults select TransformSearchResult( a ));
                    retValue.SearchResults.AddRange( from t in trackResults select TransformSearchResult( t ));

                    retValue.Success = true;
                }
                catch( Exception ex ) {
                    mLog.LogException( "Search", ex  );

                    retValue.ErrorMessage = ex.Message;
                }

                return retValue;
            });
        }
    }
}
