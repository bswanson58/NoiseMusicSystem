using System;
using Grpc.Core;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Services {
    class ArtistProvider : IArtistProvider, IDisposable {
        private readonly IServiceLocator            mServiceLocator;
        private readonly IHostInformationProvider   mHostProvider;
        private IDisposable                         mServiceLocatorSubscription;
        private IDisposable                         mHostStatusSubscription;
        private Channel                             mServiceChannel;
        private bool                                mLibraryOpen;

        public ArtistProvider( IServiceLocator serviceLocator, IHostInformationProvider hostProvider ) {
            mServiceLocator = serviceLocator;
            mHostProvider = hostProvider;

            mServiceLocatorSubscription = mServiceLocator.ChannelAcquired.Subscribe( OnChannelAcquired );
            mHostStatusSubscription = mHostProvider.HostStatus.Subscribe( OnHostStatus );
        }

        private void OnChannelAcquired( Channel channel ) {
            mServiceChannel = channel;
        }

        private void OnHostStatus( HostStatusResponse status ) {
            mLibraryOpen = status?.LibraryOpen ?? false;

            CreateClient();
        }

        private void CreateClient() {
            if( mLibraryOpen ) { }
        }

        public void Dispose() {
            mServiceLocatorSubscription?.Dispose();
            mServiceLocatorSubscription = null;
            mHostStatusSubscription?.Dispose();
            mHostStatusSubscription = null;
        }
    }
}
