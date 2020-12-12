using System;
using System.Collections.Specialized;
using System.Linq;
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
    class QueueStatusResponder : IHandle<Events.PlaybackStatusChanged> {
        private readonly IPlayQueue		                        mPlayQueue;
        private readonly INoiseLog                              mLog;
        private readonly IEventAggregator                       mEventAggregator;
        private IServerStreamWriter<QueueStatusResponse>        mStatusStream;
        private ServerCallContext                               mCallContext;
        private TaskCompletionSource<bool>                      mStatusComplete;
        private ObservableCollectionExtended<PlayQueueTrack>    mQueueList;
        private IDisposable                                     mQueueSubscription;

        public QueueStatusResponder( IPlayQueue playQueue, IEventAggregator eventAggregator, INoiseLog log ) {
            mPlayQueue = playQueue;
            mEventAggregator = eventAggregator;
            mLog = log;

            mQueueList = new ObservableCollectionExtended<PlayQueueTrack>();
            mQueueList.CollectionChanged += OnQueueChanged;
        }

        public async Task StartResponder( IServerStreamWriter<QueueStatusResponse> stream, ServerCallContext callContext ) {
            mStatusStream = stream;
            mCallContext = callContext;
            mStatusComplete = new TaskCompletionSource<bool>( false );

            mQueueSubscription = mPlayQueue.PlayQueue.AsObservableList().Connect().Bind( mQueueList ).Subscribe();

            mEventAggregator.Subscribe( this );

            await mStatusComplete.Task;
        }

        public async void Handle( Events.PlaybackStatusChanged eventArgs ) {
            await PublishStatus();
        }
        
        private async void OnQueueChanged( object sender, NotifyCollectionChangedEventArgs args ) {
            await PublishStatus();
        }

        private QueueTrackInfo TransformQueueTrack( PlayQueueTrack track ) {
            return new QueueTrackInfo {
                QueueId = track.Uid, ArtistId = track.Artist.DbId, AlbumId = track.Album.DbId, TrackId = track.Track.DbId,
                ArtistName = track.Artist.Name, AlbumName = track.Album.Name, VolumeName = track.Track.VolumeName, TrackName = track.Track.Name,
                TrackNumber = track.Track.TrackNumber, Duration = track.Track.DurationMilliseconds, Rating = track.Track.Rating, IsFavorite = track.Track.IsFavorite,
                IsPlaying = track.IsPlaying, HasPlayed = track.HasPlayed, IsFaulted = track.IsFaulted, IsStrategyQueued = track.IsStrategyQueued
            };
        }

        private async Task PublishStatus() {
            if(!mCallContext.CancellationToken.IsCancellationRequested ) {
                try {
                    var status = new QueueStatusResponse();

                    status.QueueList.AddRange( from track in mPlayQueue.PlayList select TransformQueueTrack( track ));

                    await mStatusStream.WriteAsync( status );
                }
                catch( Exception ex ) {
                    mLog.LogException( "QueueStatusResponder:PublishStatus", ex );
                }
            }
            else {
                StopResponder();
            }
        }

        public void StopResponder() {
            mQueueSubscription?.Dispose();
            mQueueSubscription = null;

            mEventAggregator.Unsubscribe( this );
            mStatusComplete.SetResult( true );
        }
    }
}
