using System;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Services {
    class QueueListProvider : BaseProvider<QueueControl.QueueControlClient>, IQueueListProvider  {
        private readonly BehaviorSubject<QueueStatusResponse>   mQueueListStatus;
        private AsyncServerStreamingCall<QueueStatusResponse>   mQueueStatusStream;
        private CancellationTokenSource                         mQueueStatusStreamCancellation;

        public  IObservable<QueueStatusResponse>                QueueListStatus => mQueueListStatus;

        public QueueListProvider( IServiceLocator serviceLocator, IHostInformationProvider hostProvider ) :
            base( serviceLocator, hostProvider ) {
            mQueueListStatus = new BehaviorSubject<QueueStatusResponse>( new QueueStatusResponse());
        }

        public async void StartQueueStatusRequests() {
            StopQueueStatusRequests();

            if( Client != null ) {
                mQueueStatusStreamCancellation = new CancellationTokenSource();
                mQueueStatusStream = Client.StartQueueStatus( new QueueControlEmpty(), cancellationToken: mQueueStatusStreamCancellation.Token );

                using( mQueueStatusStream ) {
                    try {
                        while( await mQueueStatusStream.ResponseStream.MoveNext()) {
                            PublishQueueStatus( mQueueStatusStream.ResponseStream.Current );
                        }
                    }
                    catch( RpcException ex ) {
                        if( ex.StatusCode != StatusCode.Cancelled ) {
                            var s = ex.Message;

                            // log this
                        }
                    }
                    catch( Exception ex ) {
                        var s = ex.Message;

                        // log this
                    }
                }
            }
        }

        private void PublishQueueStatus( QueueStatusResponse status ) {
            if( status != null ) {
                try {
                    mQueueListStatus.OnNext( status );
                }
                catch( Exception ex ) {
                    var s = ex.Message;

                    // log this
                }
            }
        }

        public void StopQueueStatusRequests() {
            mQueueStatusStreamCancellation?.Cancel();
            mQueueStatusStreamCancellation = null;
        }

        public async Task<bool> ClearQueue() {
            var retValue = false;

            if( Client != null ) {
                var result = await Client.ClearQueueAsync( new QueueControlEmpty());

                retValue = result.Success;
            }

            return retValue;
        }

        public async Task<bool> ClearPlayedTracks() {
            var retValue = false;

            if( Client != null ) {
                var result = await Client.ClearPlayedTracksAsync( new QueueControlEmpty());

                retValue = result.Success;
            }

            return retValue;
        }

        public async Task<bool> StartStrategyPlay() {
            var retValue = false;

            if( Client != null ) {
                var result = await Client.StartStrategyPlayAsync( new QueueControlEmpty());

                retValue = result.Success;
            }

            return retValue;
        }
    }
}
