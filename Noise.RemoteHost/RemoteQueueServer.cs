using System;
using System.Linq;
using System.ServiceModel;
using AutoMapper;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.RemoteDto;
using Noise.Infrastructure.RemoteHost;
using Noise.Infrastructure.Support;

namespace Noise.RemoteHost {
	[ServiceBehavior( InstanceContextMode = InstanceContextMode.Single )]
	public class RemoteQueueServer : INoiseRemoteQueue {
		private readonly IAlbumProvider			mAlbumProvider;
		private readonly ITrackProvider			mTrackProvider;
		private readonly IPlayController		mPlayController;
		private readonly IPlayQueue				mPlayQueue;
		private readonly IPlayStrategyFactory	mPlayStrategyFactory;
		private readonly IPlayExhaustedFactory	mPlayExhaustedFactory;

		public RemoteQueueServer( IAlbumProvider albumProvider, ITrackProvider trackProvider, IPlayController playController, IPlayQueue playQueue,
								  IPlayStrategyFactory playStrategyFactory, IPlayExhaustedFactory playExhaustedFactory ) {
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mPlayController = playController;
			mPlayQueue = playQueue;
			mPlayStrategyFactory = playStrategyFactory;
			mPlayExhaustedFactory = playExhaustedFactory;
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

		public BaseResult ExecuteQueueItemCommand( QueueItemCommand command, long itemId ) {
			var retValue = new BaseResult();

			try {
				switch( command ) {
					case QueueItemCommand.PlayNext:
						retValue.Success = mPlayQueue.ContinuePlayFromTrack( itemId );
					break;

					case QueueItemCommand.Remove:
						retValue.Success = mPlayQueue.RemoveTrack( itemId );
						break;

					case QueueItemCommand.ReplayTrack:
						retValue.Success = mPlayQueue.ReplayTrack( itemId );
						break;
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "RemoteQueueServer:ExecuteQueueItemCommand", ex );

				retValue.ErrorMessage = ex.Message;
			}
			return( retValue );
		}

		public StrategyInformationResult GetStrategyInformation() {
			var retValue = new StrategyInformationResult();

			var playStrategy = mPlayQueue.PlayStrategy;
			if( playStrategy != null ) {
				retValue.StrategyInformation.PlayStrategy = (int)playStrategy.StrategyId;

				if( playStrategy.Parameters is PlayStrategyParameterDbId ) {
					retValue.StrategyInformation.PlayStrategyParameter = ( playStrategy.Parameters as PlayStrategyParameterDbId ).DbItemId;
				}
			}

			var exhaustedStrategy = mPlayQueue.PlayExhaustedStrategy;
			if( exhaustedStrategy != null ) {
				retValue.StrategyInformation.ExhaustedStrategy = (int)exhaustedStrategy.StrategyId;

				if( exhaustedStrategy.Parameters is PlayStrategyParameterDbId ) {
					retValue.StrategyInformation.ExhaustedStrategyParameter = ( exhaustedStrategy.Parameters as PlayStrategyParameterDbId ).DbItemId;
				}
			}

			retValue.StrategyInformation.PlayStrategies = mPlayStrategyFactory.AvailableStrategies.Select( strategy => new RoQueueStrategy( strategy )).ToArray();
			retValue.StrategyInformation.ExhaustedStrategies = mPlayExhaustedFactory.AvailableStrategies.Select( strategy => new RoQueueStrategy( strategy )).ToArray();

			// Add the possible strategy parameters.

			retValue.Success = true;

			return( retValue );
		}

		public BaseResult SetQueueStrategy( int playStrategyId, long playStrategyParameter,
											int exhaustedStrategyId, long exhaustedStrategyParameter ) {
			var retValue = new BaseResult();

			try {
				var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

				var playStrategy = mPlayStrategyFactory.AvailableStrategies.FirstOrDefault( strategy => strategy.StrategyId == (ePlayStrategy)playStrategyId );
				if( playStrategy != null ) {
					var playParameters = new PlayStrategyParameterDbId( ePlayExhaustedStrategy.PlayArtist ) { DbItemId = playStrategyParameter };

					mPlayQueue.SetPlayStrategy( playStrategy.StrategyId, playParameters );

					configuration.PlayStrategy = playStrategy.StrategyId;
					configuration.PlayStrategyParameters = PlayStrategyParametersFactory.ToString( playParameters );
				}

				var exhaustedStrategy = mPlayExhaustedFactory.AvailableStrategies.FirstOrDefault( strategy => strategy.StrategyId == (ePlayExhaustedStrategy)exhaustedStrategyId );
				if( exhaustedStrategy != null ) {
					var exhaustedParameters = new PlayStrategyParameterDbId( exhaustedStrategy.StrategyId ) { DbItemId = exhaustedStrategyParameter };
	
					mPlayQueue.SetPlayExhaustedStrategy( exhaustedStrategy.StrategyId, exhaustedParameters );

					configuration.PlayExhaustedStrategy = exhaustedStrategy.StrategyId;
					configuration.PlayStrategyParameters = PlayStrategyParametersFactory.ToString( exhaustedParameters );
				}

				NoiseSystemConfiguration.Current.Save( configuration );

				retValue.Success = true;
			}
			catch( Exception ex ) {
				retValue.ErrorMessage = ex.Message;
			}

			return( retValue );
		}
	}
}
