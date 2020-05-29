using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class TransportViewModel : AutomaticPropertyBase, 
 									  IHandle<Events.PlaybackStatusChanged>, IHandle<Events.PlaybackTrackUpdated>,
									  IHandle<Events.PlaybackTrackChanged>, IHandle<Events.PlaybackInfoChanged> {
		private readonly IPlayQueue			mPlayQueue;
		private readonly IPlayController	mPlayController;

		public	DelegateCommand				Play { get; }
		public	DelegateCommand				Pause { get; }
		public	DelegateCommand				Stop {  get; }
		public	DelegateCommand				NextTrack { get; }
		public	DelegateCommand				PreviousTrack {  get; }
		public	DelegateCommand				ReplayTrack { get; }

		public TransportViewModel( IEventAggregator eventAggregator, IPlayController playController, IPlayQueue playQueue ) {
			mPlayController = playController;
			mPlayQueue = playQueue;

			eventAggregator.Subscribe( this );

			Play = new DelegateCommand( OnPlay, CanPlay );
			Pause = new DelegateCommand( OnPause, CanPause );
			Stop = new DelegateCommand( OnStop, CanStop );
			NextTrack = new DelegateCommand( OnNextTrack, CanPlayNextTrack );
			PreviousTrack = new DelegateCommand( OnPreviousTrack, CanPlayPreviousTrack );
			ReplayTrack = new DelegateCommand( OnReplayTrack, CanReplayTrack );
		}

		public ePlaybackStatus CurrentStatus {
			get { return( Get(() => CurrentStatus, ePlaybackStatus.Stopped ));  }
			set { Set(() => CurrentStatus, value ); }
		}

		public int StartTrackFlag {
			get{ return( Get( () => StartTrackFlag, 0 )); }
			set{ Set( () => StartTrackFlag, value  ); }
		}

		public int InfoUpdateFlag {
			get{ return( Get( () => InfoUpdateFlag, 0 ));  }
			set {
                Execute.OnUIThread( () => {
                    Set( () => InfoUpdateFlag, value );

					PreviousTrack.RaiseCanExecuteChanged();
                });
            }
		}

		public void Handle( Events.PlaybackStatusChanged eventArgs ) {
			if( CurrentStatus != eventArgs.Status ) {
				CurrentStatus = eventArgs.Status;
			}

            RaisePropertyChanged( () => CurrentStatus );

			Play.RaiseCanExecuteChanged();
			Pause.RaiseCanExecuteChanged();
			Stop.RaiseCanExecuteChanged();
			NextTrack.RaiseCanExecuteChanged();
			PreviousTrack.RaiseCanExecuteChanged();
			ReplayTrack.RaiseCanExecuteChanged();
		}

		public void Handle( Events.PlaybackTrackUpdated eventArgs ) {
			// Update favorite and ratings if it's the playing track.
			if(( mPlayQueue.PlayingTrack != null ) &&
			   ( mPlayQueue.PlayingTrack.Name.Equals( eventArgs.Track.Name ))) {
				StartTrackFlag++;
			}
		}

		public void Handle( Events.PlaybackTrackChanged eventArgs ) {
			StartTrackFlag++;

			ReplayTrack.RaiseCanExecuteChanged();
		}

		public void Handle( Events.PlaybackInfoChanged eventArgs ) {
			InfoUpdateFlag++;
		}

		private void OnPlay() {
			mPlayController.Play();
		}

        private bool CanPlay() {
			return( mPlayController != null && mPlayController.CanPlay );
		}

		private void OnPause() {
			mPlayController.Pause();
		}

        private bool CanPause() {
			return ( mPlayController != null && mPlayController.CanPause );
		}

		private void OnStop() {
			mPlayController.Stop();
		}

        private bool CanStop() {
			return ( mPlayController != null && mPlayController.CanStop );
		}

		private void OnNextTrack() {
			mPlayController.PlayNextTrack();
		}

        private bool CanPlayNextTrack() {
			return ( mPlayController != null && mPlayController.CanPlayNextTrack );
		}

		private void OnPreviousTrack() {
			mPlayController.PlayPreviousTrack();
		}

		private bool CanPlayPreviousTrack() {
			return mPlayController != null && mPlayController.CanPlayPreviousTrack;
		}

		private void OnReplayTrack() {
			mPlayQueue.PlayingTrackReplayCount++;

			ReplayTrack.RaiseCanExecuteChanged();
		}

		private bool CanReplayTrack() {
			return(( mPlayController.CurrentTrack != null ) &&
			       ( mPlayQueue.PlayingTrackReplayCount == 0 ));
		}
	}
}
