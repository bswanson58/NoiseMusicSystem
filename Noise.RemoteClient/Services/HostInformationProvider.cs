using System;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Services {
    class HostInformationProvider : IHostInformationProvider, IDisposable {
        private readonly BehaviorSubject<HostStatusResponse>    mHostStatus;
        private IDisposable                                     mServiceAcquiredSubscription;
        private HostInformation.HostInformationClient           mClient;
        private AsyncServerStreamingCall<HostStatusResponse>    mHostStatusStream;
        private CancellationTokenSource                         mHostStatusStreamCancellation;
        private HostStatusResponse                              mLastHostStatus;

        public  IObservable<HostStatusResponse>                 HostStatus => mHostStatus;

        public HostInformationProvider( IServiceLocator serviceLocator ) {
            mHostStatus = new BehaviorSubject<HostStatusResponse>( null );

            mServiceAcquiredSubscription = serviceLocator.ChannelAcquired.Subscribe( OnChannelAcquired );
        }

        private void OnChannelAcquired( Channel channel ) {
            mClient = null;

            if( channel != null ) {
                mClient = new HostInformation.HostInformationClient( channel );

                StartHostStatusRequests();
            }
        }

        private async void StartHostStatusRequests() {
            StopHostStatusRequests();

            if( mClient != null ) {
                mHostStatusStreamCancellation = new CancellationTokenSource();
                mHostStatusStream = mClient.StartHostStatus( new Empty(), cancellationToken: mHostStatusStreamCancellation.Token );

                using( mHostStatusStream ) {
                    try {
                        while( await mHostStatusStream.ResponseStream.MoveNext()) {
                            PublishHostStatus( mHostStatusStream.ResponseStream.Current );
                        }
                    }
                    catch( Exception ex ) { }
                }
            }
        }

        private void PublishHostStatus( HostStatusResponse status ) {
            // only publish status changes
            if( status != null ) {
                if( mLastHostStatus != null ) {
                    if(( mLastHostStatus.LibraryOpen != status.LibraryOpen ) ||
                       (!mLastHostStatus.LibraryName.Equals( status.LibraryName ))) {
                        mHostStatus.OnNext( status );
                    }
                }
                else {
                    mHostStatus.OnNext( status );
                }

                mLastHostStatus = status;
            }
        }

        public void StopHostStatusRequests() {
            mHostStatusStreamCancellation?.Cancel();
            mHostStatusStreamCancellation = null;
        }

        public async Task<HostInformationResponse> GetHostInformation() {
            if( mClient != null ) {
                return await mClient.GetHostInformationAsync( new Empty());
            }

            return default;
        }

        public void Dispose() {
            StopHostStatusRequests();

            mServiceAcquiredSubscription?.Dispose();
            mServiceAcquiredSubscription = null;
            mClient = null;
        }
    }
}
