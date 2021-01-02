﻿using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Grpc.Core;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteServer.Services {
    class PublishTransportInfo { }

    class TransportStatusResponder : IHandle<Events.PlaybackTrackChanged>, IHandle<Events.PlaybackInfoChanged>,
                                     IHandle<Events.PlaybackTrackStarted>, IHandle<Events.PlaybackTrackUpdated> {
        private readonly IEventAggregator                   mEventAggregator;
        private readonly IPlayController                    mPlayController;
        private readonly INoiseLog                          mLog;
        private readonly TransportStatus                    mTransportStatus;
        private ServerCallContext                           mCallContext;
        private IServerStreamWriter<TransportInformation>   mStatusStream;
        private BlockingCollection<PublishTransportInfo>    mPublishQueue;
        private Task                                        mPublishTask;
        private TaskCompletionSource<bool>                  mStatusComplete;
        private CancellationTokenSource                     mCancellationTokenSource;
        private IDisposable                                 mPlayStateChangedSubscription;

        public TransportStatusResponder( IPlayController playController, IEventAggregator eventAggregator, INoiseLog log ) {
            mPlayController = playController;
            mEventAggregator = eventAggregator;
            mLog = log;

            mTransportStatus = new TransportStatus();
        }

        public async Task StartResponder( IServerStreamWriter<TransportInformation> stream, ServerCallContext callContext ) {
            mStatusStream = stream;
            mCallContext = callContext;
            mPublishQueue = new BlockingCollection<PublishTransportInfo>();
            mStatusComplete = new TaskCompletionSource<bool>( false );

            mCancellationTokenSource = new CancellationTokenSource();
            mTransportStatus.UpdateTransportStatus( mPlayController.CurrentTrack );

            mPublishTask = Task.Run(() => CaptureConsumer( mCancellationTokenSource.Token ), mCancellationTokenSource.Token );

            mEventAggregator.Subscribe( this );
            mPlayStateChangedSubscription = mPlayController.PlayStateChange.Subscribe( OnPlayStateChanged );

            await mStatusComplete.Task;
            mStatusComplete = null;

            mEventAggregator.Subscribe( this );
        }

        private void OnPlayStateChanged( ePlayState toState ) {
            mTransportStatus.UpdateTransportStatus( toState );

            if( mPublishQueue?.IsCompleted == false ) {
                mPublishQueue.Add( new PublishTransportInfo());
            }
        }

        public void Handle( Events.PlaybackTrackChanged args ) {
            if( mPublishQueue?.IsCompleted == false ) {
                mPublishQueue.Add( new PublishTransportInfo());
            }
        }

        public void Handle( Events.PlaybackInfoChanged args ) {
            if( mPublishQueue?.IsCompleted == false ) {
                mPublishQueue.Add( new PublishTransportInfo());
            }
        }

        public void Handle( Events.PlaybackTrackStarted args ) {
            mTransportStatus.UpdateTransportStatus( args );

            if( mPublishQueue?.IsCompleted == false ) {
                mPublishQueue.Add( new PublishTransportInfo());
            }
        }
        public void Handle( Events.PlaybackTrackUpdated args ) {
            mTransportStatus.UpdateTransportStatus( args );

            if( mPublishQueue?.IsCompleted == false ) {
                mPublishQueue.Add( new PublishTransportInfo());
            }
        }

        private async void CaptureConsumer( CancellationToken cancelToken ) {
            while(!mPublishQueue.IsCompleted ) {
                try {
                    if( mPublishQueue.TryTake( out var _, 500, cancelToken )) {
                        while( mPublishQueue.Count > 0 ) {
                            mPublishQueue.Take();
                        }

                        await PublishStatus();
                    }

                    if( mCallContext.CancellationToken.IsCancellationRequested ) {
                        break;
                    }

                    if( cancelToken.IsCancellationRequested ) {
                        break;
                    }
                }
                catch( OperationCanceledException ) {
                    break;
                }
            }

            StopResponder();
        }

        private async Task PublishStatus() {
            if(!mCallContext.CancellationToken.IsCancellationRequested ) {
                try {
                    mTransportStatus.UpdateTrackPosition( mPlayController.PlayPosition, mPlayController.TrackEndPosition, mPlayController.PlayPositionPercentage );

                    if(!mCallContext.CancellationToken.IsCancellationRequested ) {
                        await mStatusStream.WriteAsync( mTransportStatus.CreateTransportInformation());
                    }
                }
                catch( Exception ex ) {
                    mLog.LogException( "QueueStatusResponder:PublishStatus", ex );
                }
            }
            else {
                StopResponder();
            }
        }

        private void StopResponder() {
            mPlayStateChangedSubscription?.Dispose();
            mPlayStateChangedSubscription = null;

            mCancellationTokenSource?.Cancel();
            mCancellationTokenSource = null;

            mPublishQueue?.CompleteAdding();

            mPublishTask?.Wait( 200 );
            mPublishTask = null;

            mEventAggregator.Unsubscribe( this );

            mStatusComplete?.TrySetResult( true );
        }
    }
}
