using System.Threading.Tasks;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Services {
    class TrackProvider : BaseProvider<TrackInformation.TrackInformationClient>, ITrackProvider {

        public TrackProvider( IServiceLocator serviceLocator, IHostInformationProvider hostProvider ) :
            base( serviceLocator, hostProvider ) {
        }

        public async Task<TrackListResponse> GetTrackList( long artistId, long albumId ) {
            var client = Client;

            if( client != null ) {
                return await client.GetTrackListAsync( new TrackListRequest { ArtistId = artistId, AlbumId = albumId });
            }

            return default;
        }

        public async Task<TrackListResponse> GetRatedTracks( long artistId, int includeRatingsOver, bool includeFavorites ) {
            var client = Client;

            if( client != null ) {
                return await client.GetRatedTracksAsync( 
                    new TrackRatingRequest { ArtistId = artistId, IncludeFavorites = includeFavorites, IncludeRatingsOver = includeRatingsOver});
            }

            return default;
        }

        public async Task<TrackListResponse> GetTaggedTracks( long trackId ) {
            var client = Client;

            if( client != null ) {
                return await client.GetTaggedTracksAsync( new TrackTagsRequest{ TrackId = trackId });
            }

            return default;
        }

        public async Task<TrackListResponse> GetSimilarTracks( long trackId ) {
            var client = Client;

            if( client != null ) {
                return await client.GetSimilarTracksAsync( new TrackSimilarRequest{ TrackId = trackId });
            }

            return default;
        }
    }
}
