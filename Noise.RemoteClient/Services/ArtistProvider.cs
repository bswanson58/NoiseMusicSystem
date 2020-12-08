using System;
using System.Threading.Tasks;
using Grpc.Core;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Services {
    class ArtistProvider : IArtistProvider, IDisposable {
        private ArtistInformation.ArtistInformationClient   mClient;
        private IDisposable                                 mServiceLocatorSubscription;
        private IDisposable                                 mHostStatusSubscription;
        private Channel                                     mServiceChannel;
        private bool                                        mLibraryOpen;

        public ArtistProvider( IServiceLocator serviceLocator, IHostInformationProvider hostProvider ) {
            mServiceLocatorSubscription = serviceLocator.ChannelAcquired.Subscribe( OnChannelAcquired );
            mHostStatusSubscription = hostProvider.HostStatus.Subscribe( OnHostStatus );
        }

        private void OnChannelAcquired( Channel channel ) {
            mServiceChannel = channel;

            mClient = null;
        }

        private void OnHostStatus( HostStatusResponse status ) {
            mLibraryOpen = status?.LibraryOpen ?? false;

            mClient = null;
        }

        private ArtistInformation.ArtistInformationClient Client {
            get {
                var retValue = default( ArtistInformation.ArtistInformationClient );

                if(( mServiceChannel != null ) &&
                   ( mLibraryOpen )) {
                    if( mClient == null ) {
                        mClient = new ArtistInformation.ArtistInformationClient( mServiceChannel );
                    }

                    retValue = mClient;
                }

                return retValue;
            }
        }

        public async Task<ArtistListResponse> GetArtistList() {
            var client = Client;

            if( client != null ) {
                return await mClient.GetArtistListAsync( new ArtistInfoEmpty());
            }

            return default;
        }

        public void Dispose() {
            mServiceLocatorSubscription?.Dispose();
            mServiceLocatorSubscription = null;
            mHostStatusSubscription?.Dispose();
            mHostStatusSubscription = null;
        }
    }
}
