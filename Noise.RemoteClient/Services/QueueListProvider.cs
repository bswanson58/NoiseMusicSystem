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
        private readonly IPlatformLog                           mLog;
        private AsyncServerStreamingCall<QueueStatusResponse>   mQueueStatusStream;
        private CancellationTokenSource                         mQueueStatusStreamCancellation;

        public  IObservable<QueueStatusResponse>                QueueListStatus => mQueueListStatus;

        public QueueListProvider( IServiceLocator serviceLocator, IHostInformationProvider hostProvider, IPlatformLog log ) :
            base( serviceLocator, hostProvider ) {
            mLog = log;

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
                            mLog.LogException( "StartQueueStatusRequests:RpcException", ex );
                        }
                    }
                    catch( Exception ex ) {
                        mLog.LogException( "StartQueueStatusRequest", ex );
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
                    mLog.LogException( "PublishQueueStatus", ex );
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
                try {
                    var result = await Client.ClearQueueAsync( new QueueControlEmpty());

                    retValue = result.Success;
                } 
                catch( Exception ex ) {
                    mLog.LogException( "ClearQueue", ex );
                }
            }

            return retValue;
        }

        public async Task<bool> ClearPlayedTracks() {
            var retValue = false;

            if( Client != null ) {
                try {
                    var result = await Client.ClearPlayedTracksAsync( new QueueControlEmpty());

                    retValue = result.Success;
                } 
                catch( Exception ex ) {
                    mLog.LogException( "ClearPlayedTracks", ex );
                }
            }

            return retValue;
        }

        public async Task<bool> StartStrategyPlay() {
            var retValue = false;

            if( Client != null ) {
                try {
                    var result = await Client.StartStrategyPlayAsync( new QueueControlEmpty());

                    retValue = result.Success;
                } 
                catch( Exception ex ) {
                    mLog.LogException( "StartStrategyPlay", ex );
                }
            }

            return retValue;
        }

        public async Task<bool> RemoveQueueItem( QueueTrackInfo track ) {
            var retValue = false;

            if( Client != null ) {
                try {
                    var result = await Client.RemoveQueueItemAsync( new QueueItemRequest{ ItemId = track.QueueId });

                    retValue = result.Success;
                }
                catch( Exception ex ) {
                    mLog.LogException( "RemoveQueueItem", ex );
                }
            }

            return retValue;
        }

        public async Task<bool>	PromoteQueueItem( QueueTrackInfo track ) {
            var retValue = false;

            if( Client != null ) {
                try {
                    var result = await Client.PromoteQueueItemAsync( new QueueItemRequest{ ItemId = track.QueueId });

                    retValue = result.Success;
                }
                catch( Exception ex ) {
                    mLog.LogException( "PromoteQueueItem", ex );
                }
            }

            return retValue;
        }

        public async Task<bool> ReplayQueueItem( QueueTrackInfo track ) {
            var retValue = false;

            if( Client != null ) {
                try {
                    var result = await Client.ReplayQueueItemAsync( new QueueItemRequest{ ItemId = track.QueueId });

                    retValue = result.Success;
                }
                catch( Exception ex ) {
                    mLog.LogException( "ReplayQueueItem", ex );
                }
            }

            return retValue;
        }

        public async Task<bool> SkipQueueItem( QueueTrackInfo track ) {
            var retValue = false;

            if( Client != null ) {
                try {
                    var result = await Client.SkipQueueItemAsync( new QueueItemRequest{ ItemId = track.QueueId });

                    retValue = result.Success;
                }
                catch( Exception ex ) {
                    mLog.LogException( "SkipQueueItem", ex );
                }
            }

            return retValue;
        }

        public async Task<bool> PlayFromQueueItem( QueueTrackInfo track ) {
            var retValue = false;

            if( Client != null ) {
                try {
                    var result = await Client.PlayFromQueueItemAsync( new QueueItemRequest{ ItemId = track.QueueId });

                    retValue = result.Success;
                }
                catch( Exception ex ) {
                    mLog.LogException( "PlayFromQueueItem", ex );
                }
            }

            return retValue;
        }
    }
}
