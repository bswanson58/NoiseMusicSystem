using System;
using System.Threading.Tasks;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Services {
    class ArtistProvider : BaseProvider<ArtistInformation.ArtistInformationClient>, IArtistProvider {
        private readonly IPlatformLog   mLog;

        public ArtistProvider( IServiceLocator serviceLocator, IHostInformationProvider hostProvider, IPlatformLog log ) :
            base( serviceLocator, hostProvider  ) {
            mLog = log;
        }

        public async Task<ArtistListResponse> GetArtistList() {
            var client = Client;

            if( client != null ) {
                try {
                    return await client.GetArtistListAsync( new ArtistInfoEmpty());
                }
                catch( Exception ex ) {
                    mLog.LogException( "GetArtistList", ex );
                }

                return default;
            }

            return default;
        }
    }
}
