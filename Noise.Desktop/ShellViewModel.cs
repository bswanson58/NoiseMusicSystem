using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Desktop {
	public class ShellViewModel {
		private readonly IUnityContainer	mContainer;
		private readonly IEventAggregator	mEvents;
		private INoiseManager				mNoiseManager;

		public	DelegateCommand				PlayCommand { get; private set; }
		public	DelegateCommand				PauseCommand { get; private set; }
		public	DelegateCommand				StopCommand { get; private set; }
		public	DelegateCommand				PreviousCommand { get; private set; }
		public	DelegateCommand				NextCommand { get; private set; }
		public	DelegateCommand				ReplayCommand { get; private set; }

		public ShellViewModel( IUnityContainer container ) {
			mContainer = container;
			mEvents = mContainer.Resolve<IEventAggregator>();

			mEvents.GetEvent<Events.PlaybackStatusChanged>().Subscribe( OnPlaybackStatusChanged );
			mEvents.GetEvent<Events.PlaybackTrackChanged>().Subscribe( OnPlaybackTrackChanged );
			mEvents.GetEvent<Events.PlaybackInfoChanged>().Subscribe( OnPlaybackInfoChanged );

			PlayCommand = new DelegateCommand( OnPlay, OnCanPlay );
			PauseCommand = new DelegateCommand( OnPause, OnCanPause );
			StopCommand = new DelegateCommand( OnStop, OnCanStop );
			PreviousCommand = new DelegateCommand( OnPrevious, OnCanPrevious );
			NextCommand = new DelegateCommand( OnNext, OnCanNext );
			ReplayCommand = new DelegateCommand( OnRepeat, OnCanRepeat );
		}

		private INoiseManager NoiseManager {
			get {
				if( mNoiseManager == null ) {
					mNoiseManager = mContainer.Resolve<INoiseManager>();
				}

				return( mNoiseManager );
			}
		}

		private void OnPlaybackStatusChanged( ePlaybackStatus unused ) {
			PlayCommand.RaiseCanExecuteChanged();
			PauseCommand.RaiseCanExecuteChanged();
			StopCommand.RaiseCanExecuteChanged();
			PreviousCommand.RaiseCanExecuteChanged();
			NextCommand.RaiseCanExecuteChanged();
			ReplayCommand.RaiseCanExecuteChanged();
		}

		private void OnPlaybackTrackChanged( object unused ) {
			ReplayCommand.RaiseCanExecuteChanged();
		}

		private void OnPlaybackInfoChanged( object unused ) {
			PreviousCommand.RaiseCanExecuteChanged();
		}

		private void OnPlay() {
			NoiseManager.PlayController.Play();
		}

		private bool OnCanPlay() {
			return( NoiseManager.PlayController.CanPlay );
		}

		private void OnPause() {
			NoiseManager.PlayController.Pause();
		}

		private bool OnCanPause() {
			return( NoiseManager.PlayController.CanPause );
		}

		private void OnStop() {
			NoiseManager.PlayController.Stop();
		}

		private bool OnCanStop() {
			return( NoiseManager.PlayController.CanStop );
		}

		private void OnPrevious() {
			NoiseManager.PlayController.PlayPreviousTrack();
		}

		private bool OnCanPrevious() {
			return( NoiseManager.PlayController.CanPlayPreviousTrack );
		}

		private void OnNext() {
			NoiseManager.PlayController.PlayNextTrack();
		}

		private bool OnCanNext() {
			return( NoiseManager.PlayController.CanPlayNextTrack );
		}

		private void OnRepeat() {
			NoiseManager.PlayQueue.PlayingTrackReplayCount++;
		}

		private bool OnCanRepeat() {
			var retValue = false;

			if(( NoiseManager.PlayController.CurrentTrack != null ) &&
			   ( NoiseManager.PlayQueue.PlayingTrackReplayCount == 0 )) {
				retValue = true;
			}

			return( retValue );
		}
	}
}
