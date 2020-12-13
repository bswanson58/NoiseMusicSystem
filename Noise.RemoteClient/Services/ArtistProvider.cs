using System;
using System.Threading.Tasks;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Services {
    class ArtistProvider : BaseProvider<ArtistInformation.ArtistInformationClient>, IArtistProvider {
        public ArtistProvider( IServiceLocator serviceLocator, IHostInformationProvider hostProvider ) :
            base( serviceLocator, hostProvider  ) {
        }

        public async Task<ArtistListResponse> GetArtistList() {
            var client = Client;

            if( client != null ) {
                try {
                    return await client.GetArtistListAsync( new ArtistInfoEmpty());
                }
                catch( Exception ex ) {

                }

                return default;
            }

            return default;
        }
    }
}
