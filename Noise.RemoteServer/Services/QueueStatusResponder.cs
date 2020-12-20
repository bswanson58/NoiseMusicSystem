using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using DynamicData;
using DynamicData.Binding;
using Grpc.Core;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteServer.Services {
    internal class PublishStatusInfo { }

    class QueueStatusResponder : IHandle<Events.PlaybackStatusChanged> {
        private readonly IPlayQueue		                                mPlayQueue;
        private readonly IUserTagManager                                mTagManager;
        private readonly INoiseLog                                      mLog;
        private readonly IEventAggregator                               mEventAggregator;
        private readonly ObservableCollectionExtended<PlayQueueTrack>   mQueueList;
        private BlockingCollection<PublishStatusInfo>                   mPublishQueue;
        private TaskCompletionSource<bool>                              mStatusComplete;
        private CancellationTokenSource                                 mCancellationTokenSource;
        private IServerStreamWriter<QueueStatusResponse>                mStatusStream;
        private Task                                                    mPublishTask;
        private ServerCallContext                                       mCallContext;
        private IDisposable                                             mQueueSubscription;

        public QueueStatusResponder( IPlayQueue playQueue, IUserTagManager tagManager, IEventAggregator eventAggregator, INoiseLog log ) {
            mPlayQueue = playQueue;
            mTagManager = tagManager;
            mEventAggregator = eventAggregator;
            mLog = log;

            mQueueList = new ObservableCollectionExtended<PlayQueueTrack>();
            mQueueList.CollectionChanged += OnQueueChanged;
        }

        public async Task StartResponder( IServerStreamWriter<QueueStatusResponse> stream, ServerCallContext callContext ) {
            mStatusStream = stream;
            mCallContext = callContext;
            mPublishQueue = new BlockingCollection<PublishStatusInfo>();
            mStatusComplete = new TaskCompletionSource<bool>( false );

            mQueueList.Clear();
            mQueueSubscription = mPlayQueue.PlayQueue.AsObservableList().Connect().Bind( mQueueList ).Subscribe();

            mCancellationTokenSource = new CancellationTokenSource();
            mPublishTask = Task.Run(() => CaptureConsumer( mCancellationTokenSource.Token ), mCancellationTokenSource.Token );

            mEventAggregator.Subscribe( this );

            await mStatusComplete.Task;
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

        public void Handle( Events.PlaybackStatusChanged eventArgs ) {
            if( mPublishQueue?.IsCompleted == false ) {
                mPublishQueue.Add( new PublishStatusInfo());
            }
        }
        
        private void OnQueueChanged( object sender, NotifyCollectionChangedEventArgs args ) {
            if( mPublishQueue?.IsCompleted == false ) {
                mPublishQueue.Add( new PublishStatusInfo());
            }
        }

        private QueueTrackInfo TransformQueueTrack( PlayQueueTrack track ) {
            var retValue = new QueueTrackInfo {
                QueueId = track.Uid, ArtistId = track.Artist.DbId, AlbumId = track.Album.DbId, TrackId = track.Track.DbId,
                ArtistName = track.Artist.Name, AlbumName = track.Album.Name, VolumeName = track.Track.VolumeName, TrackName = track.Track.Name,
                TrackNumber = track.Track.TrackNumber, Duration = track.Track.DurationMilliseconds, Rating = track.Track.Rating, IsFavorite = track.Track.IsFavorite,
                IsPlaying = track.IsPlaying, HasPlayed = track.HasPlayed, IsFaulted = track.IsFaulted, IsStrategyQueued = track.IsStrategyQueued
            };

            retValue.Tags.AddRange( GetTrackTags( track.Track ));

            return retValue;
        }

        private IEnumerable<QueueTagInfo> GetTrackTags( DbTrack track ) {
            var retValue = new List<QueueTagInfo>();
            var tags = mTagManager.GetAssociatedTags( track.DbId );

            retValue.AddRange( from t in tags select new QueueTagInfo{ TagId = t.DbId, TagName = t.Name });

            return retValue;
        }

        private async Task PublishStatus() {
            if(!mCallContext.CancellationToken.IsCancellationRequested ) {
                try {
                    var status = new QueueStatusResponse();
                    var totalTime = new TimeSpan();
                    var remainingTime = new TimeSpan();
                    var playList = mPlayQueue.PlayList.ToList();

                    status.QueueList.AddRange( from track in playList select TransformQueueTrack( track ));

                    playList.ForEach( track => {
                        var	trackTime = track.Track?.Duration ?? new TimeSpan();

                        totalTime = totalTime.Add( trackTime );

                        if((!track.HasPlayed ) ||
                           ( track.IsPlaying )) {
                            remainingTime = remainingTime.Add( trackTime );
                        }
                    });

                    status.TotalPlayMilliseconds = (Int32)totalTime.TotalMilliseconds;
                    status.RemainingPlayMilliseconds = (Int32)remainingTime.TotalMilliseconds;

                    if(!mCallContext.CancellationToken.IsCancellationRequested ) {
                        await mStatusStream.WriteAsync( status );
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
            mQueueSubscription?.Dispose();
            mQueueSubscription = null;

            mCancellationTokenSource?.Cancel();
            mCancellationTokenSource = null;

            mPublishQueue?.CompleteAdding();

            mPublishTask?.Wait( 200 );
            mPublishTask = null;

            mEventAggregator.Unsubscribe( this );

            mStatusComplete?.SetResult( true );
        }
    }
}
