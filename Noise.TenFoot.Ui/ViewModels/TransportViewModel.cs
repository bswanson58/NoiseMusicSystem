using System;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.TenFoot.Ui.Input;
using Noise.UI.ViewModels;

namespace Noise.TenFoot.Ui.ViewModels {
	public class TransportViewModel : PlayerViewModel,
									  IHandle<InputEvent> { //, IHandle<Infrastructure.Events.PlayExhaustedStrategyChanged> {
		private readonly IEventAggregator	mEventAggregator;
		private readonly IArtistProvider	mArtistProvider;
		private readonly IPlayQueue			mPlayQueue;
		private IStrategyDescription		mCurrentStrategy;
		private string						mCurrentStrategyTitle;

        public  string                      CurrentStrategy => ( mCurrentStrategyTitle );

		public TransportViewModel( IEventAggregator eventAggregator, IArtistProvider artistProvider,
								   IPlayQueue playQueue, IPlayController playController, IAudioController audioController ) :
			base( eventAggregator, playQueue, playController, audioController, null, null, null ) {
			mEventAggregator = eventAggregator;
			mArtistProvider = artistProvider;
			mPlayQueue = playQueue;

//			mCurrentStrategy = mPlayQueue.PlayExhaustedStrategy;
			FormatPlayStrategy( Constants.cDatabaseNullOid );

			mEventAggregator.Subscribe( this );
		}

		private void PlayOrPause() {
			if( CanExecute_Pause( null )) {
				Execute_Pause( null );
			}
			else {
				Execute_Play( null );
			}
		}

        private void FormatPlayStrategy( long strategyId ) {
			switch( mCurrentStrategy.Identifier ) {
				case eTrackPlayHandlers.PlayArtist:
					var artist = mArtistProvider.GetArtist( strategyId );
					mCurrentStrategyTitle = artist != null ? string.Format( "continuing play with tracks from '{0}'", artist.Name ) :
															 "continuing play with tracks from artist";
					break;

				case eTrackPlayHandlers.PlayFavorites:
					mCurrentStrategyTitle = "continuing play with favorite tracks";
					break;

				case eTrackPlayHandlers.Replay:
					mCurrentStrategyTitle = "continuing play by replaying queue";
					break;

				default:
					mCurrentStrategyTitle = string.Empty;
					break;
			}

			RaisePropertyChanged( () => CurrentStrategy );
		}
/*
		public void Handle( Infrastructure.Events.PlayExhaustedStrategyChanged args ) {
			mCurrentStrategy = mPlayQueue.PlayExhaustedStrategy;

			if( args.StrategyParameters is PlayStrategyParameterDbId ) {
				var dbParam = args.StrategyParameters as PlayStrategyParameterDbId;

				FormatPlayStrategy( dbParam.DbItemId );
			}
		}
*/
		public void Handle( InputEvent input ) {
			switch( input.Command ) {
				case InputCommand.Play:
				case InputCommand.Pause:
					PlayOrPause();
					break;

				case InputCommand.Stop:
					Execute_Stop( null );
					break;

				case InputCommand.NextTrack:
					Execute_NextTrack( null );
					break;

				case InputCommand.PreviousTrack:
					Execute_PreviousTrack( null );
					break;

				case InputCommand.Mute:
					Execute_Mute();
					break;

				case InputCommand.VolumeUp:
					if( Volume < 1.0d ) {
						Volume = Math.Min( 1.0D, Volume + 0.025D );
					}
					break;

				case InputCommand.VolumeDown:
					if( Volume > 0.0D ) {
						Volume = Math.Max( 0.0D, Volume - 0.025D );
					}
					break;
			}
		}
	}
}
