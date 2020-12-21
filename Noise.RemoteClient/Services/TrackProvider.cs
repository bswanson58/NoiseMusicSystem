using System;
using System.Threading.Tasks;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Services {
    class TrackProvider : BaseProvider<TrackInformation.TrackInformationClient>, ITrackProvider {
        private readonly IPlatformLog   mLog;

        public TrackProvider( IServiceLocator serviceLocator, IHostInformationProvider hostProvider, IPlatformLog log ) :
            base( serviceLocator, hostProvider ) {
            mLog = log;
        }

        public async Task<TrackListResponse> GetTrackList( long artistId, long albumId ) {
            var client = Client;

            if( client != null ) {
                try {
                    return await client.GetTrackListAsync( new TrackListRequest { ArtistId = artistId, AlbumId = albumId });
                }
                catch( Exception ex ) {
                    mLog.LogException( nameof( GetTrackList ), ex );
                }
            }

            return default;
        }

        public async Task<TrackListResponse> GetRatedTracks( long artistId, int includeRatingsOver, bool includeFavorites ) {
            var client = Client;

            if( client != null ) {
                try {
                    return await client.GetRatedTracksAsync( 
                        new TrackRatingRequest { ArtistId = artistId, IncludeFavorites = includeFavorites, IncludeRatingsOver = includeRatingsOver});
                }
                catch( Exception ex ) {
                    mLog.LogException( nameof( GetRatedTracks ), ex );
                }
            }

            return default;
        }

        public async Task<TrackListResponse> GetTaggedTracks( long trackId ) {
            var client = Client;

            if( client != null ) {
                try {
                    return await client.GetTaggedTracksAsync( new TrackTagsRequest{ TrackId = trackId });
                }
                catch( Exception ex ) {
                    mLog.LogException( nameof( GetTaggedTracks ), ex );
                }
            }

            return default;
        }

        public async Task<TrackListResponse> GetSimilarTracks( long trackId ) {
            var client = Client;

            if( client != null ) {
                try {
                    return await client.GetSimilarTracksAsync( new TrackSimilarRequest{ TrackId = trackId });
                }
                catch( Exception ex ) {
                    mLog.LogException( nameof( GetSimilarTracks ), ex );
                }
            }

            return default;
        }

        public async Task<TrackListResponse> GetFavoriteTracks() {
            var client = Client;

            if( client != null ) {
                try {
                    return await client.GetFavoriteTracksAsync( new TrackInfoEmpty());
                }
                catch( Exception ex ) {
                    mLog.LogException( nameof( GetFavoriteTracks ), ex );
                }
            }

            return default;
        }

        public async Task<TrackUpdateResponse> UpdateTrackRatings( TrackInfo track ) {
            var client = Client;

            if( client != null ) {
                try {
                    return await client.UpdateTrackRatingsAsync( new TrackUpdateRequest { Track = track });
                }
                catch( Exception ex ) {
                    mLog.LogException( nameof( UpdateTrackRatings ), ex );
                }
            }

            return default;
        }
    }
}
