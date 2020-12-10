using System;
using Grpc.Core;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Services {
    class BaseProvider<T> : IDisposable where T: ClientBase<T> {
        private ClientBase<T>   mClient;
        private IDisposable     mServiceLocatorSubscription;
        private IDisposable     mHostStatusSubscription;
        private Channel         mServiceChannel;
        private bool            mLibraryOpen;

        protected BaseProvider( IServiceLocator serviceLocator, IHostInformationProvider hostProvider ) {
            mServiceLocatorSubscription = serviceLocator.ChannelAcquired.Subscribe( OnChannelAcquired );
            mHostStatusSubscription = hostProvider.HostStatus.Subscribe( OnHostStatus );
        }

        private void OnChannelAcquired( Channel channel ) {
            mServiceChannel = channel;

            mClient = default;
        }

        private void OnHostStatus( HostStatusResponse status ) {
            mLibraryOpen = status?.LibraryOpen ?? false;

            mClient = default;
        }

        protected T Client {
            get {
                var retValue = default( T );

                if(( mServiceChannel != null ) &&
                   ( mLibraryOpen )) {
                    if( mClient == null ) {
                        mClient = (T)Activator.CreateInstance( typeof( T ), mServiceChannel );
                    }

                    retValue = mClient as T;
                }

                return retValue;
            }
        }

        public void Dispose() {
            mServiceLocatorSubscription?.Dispose();
            mServiceLocatorSubscription = null;
            mHostStatusSubscription?.Dispose();
            mHostStatusSubscription = null;
        }
    }
}
