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
    }
}
