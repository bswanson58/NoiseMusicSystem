using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RemoteServer.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteServer.Services {
    class QueueService : QueueControl.QueueControlBase {
        private readonly IRemoteServiceFactory      mServiceFactory;
        private readonly IAlbumProvider			    mAlbumProvider;
        private readonly ITrackProvider			    mTrackProvider;
        private readonly IPlayCommand			    mPlayCommand;
        private readonly IPlayQueue				    mPlayQueue;
        private readonly INoiseLog				    mLog;
        private QueueStatusResponder                mQueueStatusResponder;

        public QueueService( IRemoteServiceFactory serviceFactory, IAlbumProvider albumProvider, ITrackProvider trackProvider,
                             IPlayQueue playQueue,IPlayCommand playCommand, INoiseLog log ) {
            mServiceFactory = serviceFactory;
            mAlbumProvider = albumProvider;
            mTrackProvider = trackProvider;
            mPlayCommand = playCommand;
            mPlayQueue = playQueue;
            mLog = log;
        }

        public override Task<QueueControlResponse> AddTrack( AddQueueRequest request, ServerCallContext context ) {
            return Task.Run( () => {
                var retValue = new QueueControlResponse();

                try {
                    var track = mTrackProvider.GetTrack( request.ItemId );

                    if( track != null ) {
                        mPlayCommand.Play( track );

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
            return new QueueTrackInfo {
                QueueId = track.Uid, ArtistId = track.Artist.DbId, AlbumId = track.Album.DbId, TrackId = track.Track.DbId,
                ArtistName = track.Artist.Name, AlbumName = track.Album.Name, VolumeName = track.Track.VolumeName, TrackName = track.Track.Name,
                TrackNumber = track.Track.TrackNumber, Duration = track.Track.DurationMilliseconds, Rating = track.Track.Rating, IsFavorite = track.Track.IsFavorite,
                IsPlaying = track.IsPlaying, HasPlayed = track.HasPlayed, IsFaulted = track.IsFaulted, IsStrategyQueued = track.IsStrategyQueued
            };
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
            StopHostStatusResponder();

            mQueueStatusResponder = mServiceFactory.QueueStatusResponder;
            await mQueueStatusResponder.StartResponder( responseStream, context );
        }

        public void StopHostStatusResponder() {
            mQueueStatusResponder?.StopResponder();
            mQueueStatusResponder = null;
        }
    }
}
