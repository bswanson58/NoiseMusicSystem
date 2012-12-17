using System;
using Caliburn.Micro;
using Noise.Infrastructure.Interfaces;
using Noise.TenFoot.Ui.Input;
using Noise.UI.ViewModels;

namespace Noise.TenFoot.Ui.ViewModels {
	public class TransportViewModel : PlayerViewModel,
									  IHandle<InputEvent>, IHandle<Infrastructure.Events.PlayExhaustedChanged> {
		private readonly IEventAggregator	mEventAggregator;
		private readonly IPlayQueue			mPlayQueue;
		private ePlayExhaustedStrategy		mCurrentStrategy;
		private string						mCurrentStrategyTitle;

		public TransportViewModel( IEventAggregator eventAggregator, IPlayQueue playQueue, IPlayController playController ) :
			base( eventAggregator, playQueue, playController ) {
			mEventAggregator = eventAggregator;
			mPlayQueue = playQueue;

			mCurrentStrategy = mPlayQueue.PlayExhaustedStrategy;
			FormatPlayStrategy();

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

		public string CurrentStrategy {
			get{ return( mCurrentStrategyTitle ); }
		}

		private void FormatPlayStrategy() {
			switch( mCurrentStrategy ) {
				case ePlayExhaustedStrategy.PlayArtist:
					mCurrentStrategyTitle = "continuing play with tracks from artist";
					break;

				case ePlayExhaustedStrategy.PlayFavorites:
					mCurrentStrategyTitle = "continuing play with favorite tracks";
					break;

				case ePlayExhaustedStrategy.Replay:
					mCurrentStrategyTitle = "continuing play by replaying queue";
					break;

				default:
					mCurrentStrategyTitle = string.Empty;
					break;
			}

			RaisePropertyChanged( () => CurrentStrategy );
		}

		public void Handle( Infrastructure.Events.PlayExhaustedChanged args ) {
			mCurrentStrategy = mPlayQueue.PlayExhaustedStrategy;

			FormatPlayStrategy();
		}

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
