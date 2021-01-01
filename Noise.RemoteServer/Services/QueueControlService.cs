using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RemoteServer.Interfaces;
using Noise.RemoteServer.Protocol;
using ReusableBits.ExtensionClasses.MoreLinq;

namespace Noise.RemoteServer.Services {
    class QueueService : QueueControl.QueueControlBase {
        private readonly IRemoteServiceFactory      mServiceFactory;
        private readonly IAlbumProvider			    mAlbumProvider;
        private readonly ITrackProvider			    mTrackProvider;
        private readonly IUserTagManager            mTagManager;
        private readonly IPlayCommand			    mPlayCommand;
        private readonly IPlayQueue				    mPlayQueue;
        private readonly INoiseLog				    mLog;

        public QueueService( IRemoteServiceFactory serviceFactory, IAlbumProvider albumProvider, ITrackProvider trackProvider,
                             IUserTagManager tagManager, IPlayQueue playQueue,IPlayCommand playCommand, INoiseLog log ) {
            mServiceFactory = serviceFactory;
            mAlbumProvider = albumProvider;
            mTrackProvider = trackProvider;
            mTagManager = tagManager;
            mPlayCommand = playCommand;
            mPlayQueue = playQueue;
            mLog = log;
        }

        // duplicated from AlbumTracksViewModel
        private IEnumerable<DbTrack> QueueTracksWithStrategy( long fromAlbum, long trackId ) {
            var retValue = new List<DbTrack>();

            using( var tracksList = mTrackProvider.GetTrackList( fromAlbum )) {
                var targetTrack = tracksList.List.FirstOrDefault( t => t.DbId.Equals( trackId ));
                var previousTrack = targetTrack;
                var previousTracks = new List<DbTrack>();

                while(( previousTrack != null ) && 
                      (( previousTrack.PlayAdjacentStrategy == ePlayAdjacentStrategy.PlayPrevious ) ||
                       ( previousTrack.PlayAdjacentStrategy == ePlayAdjacentStrategy.PlayNextPrevious ))) {
                    previousTrack = tracksList.List.TakeWhile( t => !t.DbId.Equals( previousTrack.DbId )).LastOrDefault();

                    if( previousTrack != null ) {
                        previousTracks.Insert( 0, previousTrack );
                    }
                }
                previousTracks.ForEach( t => retValue.Add( mTrackProvider.GetTrack( t.DbId )));
            
                retValue.Add( mTrackProvider.GetTrack( trackId ));

                while(( targetTrack != null ) &&
                      (( targetTrack.PlayAdjacentStrategy == ePlayAdjacentStrategy.PlayNextPrevious ) ||
                       ( targetTrack.PlayAdjacentStrategy == ePlayAdjacentStrategy.PlayNext ))) {
                    targetTrack = tracksList.List.SkipWhile( t => !t.DbId.Equals( targetTrack.DbId )).Skip( 1 ).FirstOrDefault();

                    if( targetTrack != null ) {
                        retValue.Add( mTrackProvider.GetTrack( targetTrack.DbId ));
                    }
                }
            }

            return retValue;
        }

        public override Task<QueueControlResponse> AddTrack( AddQueueRequest request, ServerCallContext context ) {
            return Task.Run( () => {
                var retValue = new QueueControlResponse();

                try {
                    var track = mTrackProvider.GetTrack( request.ItemId );

                    if( track != null ) {
                        if( request.PlayNext ) {
                            QueueTracksWithStrategy( track.Album, track.DbId ).Reverse().ForEach( t => mPlayCommand.PlayNext( t ));
                        }
                        else {
                            mPlayCommand.Play( QueueTracksWithStrategy( track.Album, track.DbId ));
                        }

                        retValue.Success = true;
                    }
                }
                catch( Exception ex ) {
                    mLog.LogException( $"AddTrack:{request.ItemId}", ex );

                    retValue.ErrorMessage = ex.Message;
                }

                return retValue;
            });
        }

        public override Task<QueueControlResponse> AddAlbum( AddQueueRequest request, ServerCallContext context ) {
            return Task.Run( () => {
                var retValue = new QueueControlResponse();

                try {
                    var album = mAlbumProvider.GetAlbum( request.ItemId );

                    if( album != null ) {
                        mPlayCommand.Play( album );

                        retValue.Success = true;
                    }
                }
                catch( Exception ex ) {
                    mLog.LogException( $"AddAlbum:{request.ItemId}", ex );

                    retValue.ErrorMessage = ex.Message;
                }

                return retValue;
            });
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

        public override Task<QueueListResponse> GetQueueList( QueueControlEmpty request, ServerCallContext context ) {
            return Task.Run( () => {
                var retValue = new QueueListResponse();

                try {
                    retValue.QueueList.AddRange( mPlayQueue.PlayList.Select( TransformQueueTrack ));

                    retValue.Success = true;
                }
                catch( Exception ex ) {
                    mLog.LogException( "GetQueueList", ex );

                    retValue.ErrorMessage = ex.Message;
                }

                return retValue;
            });
        }

        public override async Task StartQueueStatus( QueueControlEmpty request, IServerStreamWriter<QueueStatusResponse> responseStream, ServerCallContext context ) {
            var queueStatusResponder = mServiceFactory.QueueStatusResponder;

            await queueStatusResponder.StartResponder( responseStream, context );
        }

        public override Task<QueueControlResponse> ClearQueue( QueueControlEmpty request, ServerCallContext context ) {
            return Task.Run( () => {
                var retValue = new QueueControlResponse();

                try {
                    mPlayQueue.ClearQueue();

                    retValue.Success = true;
                }
                catch( Exception ex ) {
                    mLog.LogException( "ClearQueue", ex );

                    retValue.ErrorMessage = ex.Message;
                }

                return retValue;
            });
        }

        public override Task<QueueControlResponse> ClearPlayedTracks( QueueControlEmpty request, ServerCallContext context ) {
            return Task.Run( () => {
                var retValue = new QueueControlResponse();

                try {
                    mPlayQueue.RemovePlayedTracks();

                    retValue.Success = true;
                }
                catch( Exception ex ) {
                    mLog.LogException( "ClearPlayedTracks", ex );

                    retValue.ErrorMessage = ex.Message;
                }

                return retValue;
            });
        }

        public override Task<QueueControlResponse> StartStrategyPlay( QueueControlEmpty request, ServerCallContext context ) {
            return Task.Run( () => {
                var retValue = new QueueControlResponse();

                try {
                    mPlayQueue.StartPlayStrategy();

                    retValue.Success = true;
                }
                catch( Exception ex ) {
                    mLog.LogException( "StartStrategyPlay", ex );

                    retValue.ErrorMessage = ex.Message;
                }

                return retValue;
            });
        }

        public override Task<QueueControlResponse> PlayFromQueueItem( QueueItemRequest request, ServerCallContext context ) {
            return Task.Run( () => {
                var retValue = new QueueControlResponse();

                try {
                    retValue.Success = mPlayQueue.ContinuePlayFromTrack( request.ItemId );
                }
                catch( Exception ex ) {
                    mLog.LogException( "PlayFromQueueItem", ex );

                    retValue.ErrorMessage = ex.Message;
                }
                return retValue;
            });
        }

        public override Task<QueueControlResponse> PromoteQueueItem( QueueItemRequest request, ServerCallContext context ) {
            return Task.Run( () => {
                var retValue = new QueueControlResponse();

                try {
                    mPlayQueue.PromoteTrackFromStrategy( request.ItemId );

                    retValue.Success = true;
                }
                catch( Exception ex ) {
                    mLog.LogException( "PromoteQueueItem", ex );

                    retValue.ErrorMessage = ex.Message;
                }
                return retValue;
            });
        }

        public override Task<QueueControlResponse> RemoveQueueItem( QueueItemRequest request, ServerCallContext context ) {
            return Task.Run( () => {
                var retValue = new QueueControlResponse();

                try {
                    retValue.Success = mPlayQueue.RemoveTrack( request.ItemId );
                }
                catch( Exception ex ) {
                    mLog.LogException( "RemoveQueueItem", ex );

                    retValue.ErrorMessage = ex.Message;
                }
                return retValue;
            });
        }

        public override Task<QueueControlResponse> ReplayQueueItem( QueueItemRequest request, ServerCallContext context ) {
            return Task.Run( () => {
                var retValue = new QueueControlResponse();

                try {
                    retValue.Success = mPlayQueue.ReplayTrack( request.ItemId );
                }
                catch( Exception ex ) {
                    mLog.LogException( "ReplayQueueItem", ex );

                    retValue.ErrorMessage = ex.Message;
                }
                return retValue;
            });
        }

        public override Task<QueueControlResponse> SkipQueueItem( QueueItemRequest request, ServerCallContext context ) {
            return Task.Run( () => {
                var retValue = new QueueControlResponse();

                try {
                    retValue.Success = mPlayQueue.SkipTrack( request.ItemId );
                }
                catch( Exception ex ) {
                    mLog.LogException( "SkipQueueItem", ex );

                    retValue.ErrorMessage = ex.Message;
                }
                return retValue;
            });
        }
    }
}
