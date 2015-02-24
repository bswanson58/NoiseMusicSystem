using System;
using System.Collections.Generic;
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
		private readonly IArtistProvider		mArtistProvider;
		private readonly IAlbumProvider			mAlbumProvider;
		private readonly ITrackProvider			mTrackProvider;
		private readonly IGenreProvider			mGenreProvider;
		private readonly IPlayController		mPlayController;
		private readonly IPlayQueue				mPlayQueue;
		private readonly IPlayStrategyFactory	mPlayStrategyFactory;
		private readonly IPlayExhaustedFactory	mPlayExhaustedFactory;
		private readonly INoiseLog				mLog;

		public RemoteQueueServer( IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider, IGenreProvider genreProvider,
								  IPlayController playController, IPlayQueue playQueue,
								  IPlayStrategyFactory playStrategyFactory, IPlayExhaustedFactory playExhaustedFactory, INoiseLog log ) {
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mGenreProvider = genreProvider;
			mPlayController = playController;
			mPlayQueue = playQueue;
			mPlayStrategyFactory = playStrategyFactory;
			mPlayExhaustedFactory = playExhaustedFactory;
			mLog = log;
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
				mLog.LogException( string.Format( "EnqueueTrack:{0}", trackId ), ex );

				retValue.ErrorMessage = ex.Message;
			}

			return( retValue );
		}

		public BaseResult EnqueueTrackList( long[] trackIds ) {
			var retValue = new BaseResult();

			try {
				var trackList = trackIds.Select( trackId => mTrackProvider.GetTrack( trackId )).Where( track => track != null ).ToList();

				if( trackList.Any()) {
					mPlayQueue.Add( trackList );

					retValue.Success = true;
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "EnqueueTrackList", ex );

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
				mLog.LogException( string.Format( "EnqueueAlbum:{0}", albumId ), ex );

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
				mLog.LogException( "GetQueuedTrackList", ex );

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

					case TransportCommand.Repeat:
						if( mPlayController.CurrentTrack != null ) {
							mPlayQueue.PlayingTrackReplayCount = 1;
						}
						break;
				}

				retValue.Success = true;
			}
			catch( Exception ex ) {
				mLog.LogException( string.Format( "ExecuteTransportCommand:{0}", command ), ex );

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
				mLog.LogException( string.Format( "ExecuteQueueCommand:{0}", command ), ex );

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
				mLog.LogException( string.Format( "ExecuteQueueItemCommand:{0}", command ), ex );

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
			using( var artistList = mArtistProvider.GetArtistList()) {
				var parameterList = new List<RoStrategyParameter>();

				parameterList.AddRange( artistList.List.Select( artist => new RoStrategyParameter{ Id = artist.DbId, Title = artist.Name } ));
				retValue.StrategyInformation.ArtistParameters = parameterList.ToArray();

				parameterList.Clear();

				using( var genreList = mGenreProvider.GetGenreList()) {
					var uniqueGenres = genreList.List.Join( artistList.List, genre => genre.DbId, artist => artist.Genre, ( genre, artist ) => genre ).Distinct();

					parameterList.AddRange( uniqueGenres.Select( genre => new RoStrategyParameter { Id = genre.DbId, Title = genre.Name }));

					retValue.StrategyInformation.GenreParameters = parameterList.ToArray();
				}
			}


			retValue.Success = true;

			return( retValue );
		}

		public BaseResult SetQueueStrategy( int playStrategyId, long playStrategyParameter,
											int exhaustedStrategyId, long exhaustedStrategyParameter ) {
			var retValue = new BaseResult();

			try {
				var playStrategy = mPlayStrategyFactory.AvailableStrategies.FirstOrDefault( strategy => strategy.StrategyId == (ePlayStrategy)playStrategyId );
				if( playStrategy != null ) {
					var playParameters = new PlayStrategyParameterDbId( ePlayExhaustedStrategy.PlayArtist ) { DbItemId = playStrategyParameter };

					mPlayQueue.SetPlayStrategy( playStrategy.StrategyId, playParameters );
				}

				var exhaustedStrategy = mPlayExhaustedFactory.AvailableStrategies.FirstOrDefault( strategy => strategy.StrategyId == (ePlayExhaustedStrategy)exhaustedStrategyId );
				if( exhaustedStrategy != null ) {
					var exhaustedParameters = new PlayStrategyParameterDbId( exhaustedStrategy.StrategyId ) { DbItemId = exhaustedStrategyParameter };
	
					mPlayQueue.SetPlayExhaustedStrategy( exhaustedStrategy.StrategyId, exhaustedParameters );
				}

				retValue.Success = true;
			}
			catch( Exception ex ) {
				retValue.ErrorMessage = ex.Message;
			}

			return( retValue );
		}
	}
}
