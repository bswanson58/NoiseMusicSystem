using System.Threading.Tasks;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Services {
    class AlbumProvider : BaseProvider<AlbumInformation.AlbumInformationClient>, IAlbumProvider {
        public AlbumProvider( IServiceLocator serviceLocator, IHostInformationProvider hostProvider ) :
            base( serviceLocator, hostProvider ) {
        }

        public async Task<AlbumListResponse> GetAlbumList( long artistId ) {
            var client = Client;

            if( client != null ) {
                return await client.GetAlbumListAsync( new AlbumListRequest { ArtistId = artistId });
            }

            return default;
        }
    }
}
