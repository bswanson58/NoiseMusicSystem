using System;
using System.Windows.Input;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Presentation.Commands;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.UI.ViewModels {
	class PlayerViewModel : BindableObject {
		private	readonly IUnityContainer	mContainer;
		private readonly IEventAggregator	mEvents;
		private readonly INoiseManager		mNoiseManager;
		private DbTrack						mCurrentTrack;
		private StorageFile					mCurrentFile;

		private readonly DelegateCommand<object>	mPlayCommand;
		private readonly DelegateCommand<object>	mPauseCommand;

		public PlayerViewModel( IUnityContainer container ) {
			mContainer = container;
			mEvents = mContainer.Resolve<IEventAggregator>();
			mNoiseManager = mContainer.Resolve<INoiseManager>();

			mEvents.GetEvent<Events.TrackSelected>().Subscribe( OnTrackSelected );

			mPlayCommand = new DelegateCommand<object>( OnPlay, CanPlay );
			mPauseCommand = new DelegateCommand<object>( OnPause, CanPause );
		}

		public string TrackName {
			get {
				var retValue = "None";

				if( mCurrentTrack != null ) {
					retValue = mCurrentTrack.Name;
				}

				return( retValue );
			} 
		}

		public TimeSpan TrackPosition {
			get{ return( mNoiseManager.AudioPlayer.PlayPosition ); }
		}

		public void OnTrackSelected( DbTrack track ) {
			mCurrentTrack = track;
			mCurrentFile = mNoiseManager.DataProvider.GetPhysicalFile( mCurrentTrack );

			NotifyOfPropertyChange( () => TrackName );

			if( mNoiseManager.AudioPlayer.OpenFile( mCurrentFile )) {
				mNoiseManager.AudioPlayer.Play();

				mPlayCommand.RaiseCanExecuteChanged();
				mPauseCommand.RaiseCanExecuteChanged();
			}
		}

		private void OnPlay( object sender ) {
			mNoiseManager.AudioPlayer.Play();

			NotifyOfPropertyChange( () => TrackPosition );
		}
		private bool CanPlay( object sender ) {
			return( mNoiseManager.AudioPlayer.IsOpen );
		}
		public ICommand PlayCommand {
			get{ return( mPlayCommand ); }
		}

		private void OnPause( object sender ) {
			mNoiseManager.AudioPlayer.Pause();

			NotifyOfPropertyChange( () => TrackPosition );
		}
		private bool CanPause( object sender ) {
			return( mNoiseManager.AudioPlayer.IsOpen );
		}
		public ICommand PauseCommand {
			get{ return( mPauseCommand ); }
		}
	}
}
