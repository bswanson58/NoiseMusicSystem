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
		private readonly IDataProvider		mDataProvider;
		private readonly IPlayController	mPlayController;
		private readonly IPlayQueue			mPlayQueue;

		public RemoteQueueServer( IDataProvider dataProvider, IPlayController playController, IPlayQueue playQueue ) {
			mDataProvider = dataProvider;
			mPlayController = playController;
			mPlayQueue = playQueue;
		}

		public BaseResult EnqueueTrack( long trackId ) {
			var retValue = new BaseResult();

			try {
				var track = mDataProvider.GetTrack( trackId );

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
				var album = mDataProvider.GetAlbum( albumId );

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
	}
}
