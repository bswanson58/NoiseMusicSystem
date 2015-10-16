using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class TransportViewModel : AutomaticCommandBase, 
										IHandle<Events.PlaybackStatusChanged>,
										IHandle<Events.PlaybackTrackUpdated>,
										IHandle<Events.PlaybackTrackChanged>,
										IHandle<Events.PlaybackInfoChanged> {
		private readonly IPlayQueue			mPlayQueue;
		private readonly IPlayController	mPlayController;

		public TransportViewModel( IEventAggregator eventAggregator, IPlayController playController, IPlayQueue playQueue ) {
			mPlayController = playController;
			mPlayQueue = playQueue;

			eventAggregator.Subscribe( this );
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
			set{ Execute.OnUIThread( () => Set( () => InfoUpdateFlag, value )); }
		}

		public void Handle( Events.PlaybackStatusChanged eventArgs ) {
			if( CurrentStatus != eventArgs.Status ) {
				CurrentStatus = eventArgs.Status;
			}
			else {
				RaisePropertyChanged( () => CurrentStatus );
			}
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
		}

		public void Handle( Events.PlaybackInfoChanged eventArgs ) {
			InfoUpdateFlag++;
		}

		public void Execute_Play( object sender ) {
			mPlayController.Play();
		}
		[DependsUpon( "CurrentStatus" )]
		public bool CanExecute_Play( object sender ) {
			return( mPlayController != null && mPlayController.CanPlay );
		}

		public void Execute_Pause( object sender ) {
			mPlayController.Pause();
		}
		[DependsUpon( "CurrentStatus" )]
		public bool CanExecute_Pause( object sender ) {
			return ( mPlayController != null && mPlayController.CanPause );
		}

		public void Execute_Stop( object sender ) {
			mPlayController.Stop();
		}
		[DependsUpon( "CurrentStatus" )]
		public bool CanExecute_Stop( object sender ) {
			return ( mPlayController != null && mPlayController.CanStop );
		}

		public void Execute_NextTrack( object sender ) {
			mPlayController.PlayNextTrack();
		}
		[DependsUpon( "CurrentStatus" )]
		public bool CanExecute_NextTrack( object sender ) {
			return ( mPlayController != null && mPlayController.CanPlayNextTrack );
		}

		public void Execute_PreviousTrack( object sender ) {
			mPlayController.PlayPreviousTrack();
		}
		[DependsUpon( "CurrentStatus" )]
		[DependsUpon( "InfoUpdateFlag" )]
		public bool CanExecute_PreviousTrack( object sender ) {
			return ( mPlayController != null && mPlayController.CanPlayPreviousTrack );
		}

		public void Execute_ReplayTrack() {
			mPlayQueue.PlayingTrackReplayCount++;

			RaiseCanExecuteChangedEvent( "CanExecute_ReplayTrack" );
		}
		[DependsUpon( "CurrentStatus" )]
		[DependsUpon( "StartTrackFlag" )]
		public bool CanExecute_ReplayTrack() {
			return(( mPlayController.CurrentTrack != null ) &&
			       ( mPlayQueue.PlayingTrackReplayCount == 0 ));
		}
	}
}
