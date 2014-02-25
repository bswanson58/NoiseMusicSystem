using System;
using System.Linq;
using System.ServiceModel;
using AutoMapper;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.RemoteDto;
using Noise.Infrastructure.RemoteHost;

namespace Noise.RemoteHost {
	[ServiceBehavior( InstanceContextMode = InstanceContextMode.Single )]
	public class RemoteQueueServer : INoiseRemoteQueue {
		private readonly IAlbumProvider		mAlbumProvider;
		private readonly ITrackProvider		mTrackProvider;
		private readonly IPlayController	mPlayController;
		private readonly IPlayQueue			mPlayQueue;

		public RemoteQueueServer( IAlbumProvider albumProvider, ITrackProvider trackProvider, IPlayController playController, IPlayQueue playQueue ) {
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mPlayController = playController;
			mPlayQueue = playQueue;
		}

		public BaseResult EnqueueTrack( long trackId ) {
			var retValue = new BaseResult();

			try {
				var track = mTrackProvider.GetTrack( trackId );

				if( track != null ) {
					GlobalCommands.PlayTrack.Execute( track );

					retValue.Success = true;
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "RemoteQueueServer:EnqueueTrack", ex );

				retValue.ErrorMessage = ex.Message;
			}

			return( retValue );
		}

		public BaseResult EnqueueAlbum( long albumId ) {
			var retValue = new BaseResult();

			try {
				var album = mAlbumProvider.GetAlbum( albumId );

				if( album != null ) {
					GlobalCommands.PlayAlbum.Execute( album );

					retValue.Success = true;
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "RemoteQueueServer:EnqueueAlbum", ex );

				retValue.ErrorMessage = ex.Message;
			}

			return( retValue );
		}

		private static RoPlayQueueTrack TransformQueueTrack( PlayQueueTrack queueTrac ) {
			var retValue = new RoPlayQueueTrack();

			Mapper.DynamicMap( queueTrac, retValue );

			return( retValue );
		}

		public PlayQueueListResult GetQueuedTrackList() {
			var retValue = new PlayQueueListResult();

			try {
				retValue.Tracks = mPlayQueue.PlayList.Select( TransformQueueTrack).ToArray();

				retValue.Success = true;
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "RemoteQueueServer:GetQueuedTrackList", ex );

				retValue.ErrorMessage = ex.Message;
			}

			return( retValue );
		}

		public BaseResult ExecuteTransportCommand( TransportCommand command ) {
			var retValue = new BaseResult();

			try {
				switch( command ) {
					case TransportCommand.Play:
						if( mPlayController.CanPlay ) {
							mPlayController.Play();
						}
						break;

					case TransportCommand.Pause:
						if( mPlayController.CanPause ) {
							mPlayController.Pause();
						}
						break;

					case TransportCommand.PlayNext:
						if( mPlayController.CanPlayNextTrack ) {
							mPlayController.PlayNextTrack();
						}
						break;

					case TransportCommand.PlayPrevious:
						if( mPlayController.CanPlayPreviousTrack ) {
							mPlayController.PlayPreviousTrack();
						}
						break;

					case TransportCommand.Stop:
						if( mPlayController.CanStop ) {
							mPlayController.Stop();
						}
						break;
				}

				retValue.Success = true;
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "RemoteQueueServer:ExecuteTransportCommand", ex );

				retValue.ErrorMessage = ex.Message;
			}

			return( retValue );
		}

		public BaseResult ExecuteQueueCommand( QueueCommand command ) {
			var retValue = new BaseResult();

			try {
				switch( command ) {
					case QueueCommand.Clear:
						mPlayQueue.ClearQueue();
						break;

					case QueueCommand.ClearPlayed:
						mPlayQueue.RemovePlayedTracks();
						break;

					case QueueCommand.StartPlaying:
						mPlayQueue.StartPlayStrategy();
						break;
				}

				retValue.Success = true;
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "RemoteQueueServer:ExecuteQueueCommand", ex );

				retValue.ErrorMessage = ex.Message;
			}

			return( retValue );
		}
	}
}
