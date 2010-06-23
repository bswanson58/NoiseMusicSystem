using System;
using System.Collections.Generic;
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
		private PlayQueueTrack				mCurrentTrack;
		private int							mCurrentChannel;
		private readonly Dictionary<int, PlayQueueTrack>	mOpenTracks;

		private readonly DelegateCommand<object>	mPlayCommand;
		private readonly DelegateCommand<object>	mPauseCommand;

		public PlayerViewModel( IUnityContainer container ) {
			mContainer = container;
			mEvents = mContainer.Resolve<IEventAggregator>();
			mNoiseManager = mContainer.Resolve<INoiseManager>();
			mOpenTracks = new Dictionary<int, PlayQueueTrack>();

			mEvents.GetEvent<Events.TrackSelected>().Subscribe( OnTrackSelected );
			mEvents.GetEvent<Events.PlayQueueChanged>().Subscribe( OnPlayQueueChanged );

			mPlayCommand = new DelegateCommand<object>( OnPlay, CanPlay );
			mPauseCommand = new DelegateCommand<object>( OnPause, CanPause );
		}

		public string TrackName {
			get {
				var retValue = "None";

				if( mCurrentTrack != null ) {
					retValue = mCurrentTrack.Track.Name;
				}

				return( retValue );
			} 
		}

		public TimeSpan TrackPosition {
			get {
				var	retValue = new TimeSpan();

				if( mCurrentTrack != null ) {
					retValue = mNoiseManager.AudioPlayer.PlayPosition( mCurrentChannel );
				}

				return( retValue );
			}
		}

		public void OnTrackSelected( DbTrack track ) {
			mNoiseManager.PlayQueue.Add( track );
		}

		public void OnPlayQueueChanged( IPlayQueue playQueue ) {
			if( mCurrentTrack == null ) {
				StartPlaying();
			}
		}

		private void StartPlaying() {
			mCurrentTrack = mNoiseManager.PlayQueue.PlayNextTrack();
			if( mCurrentTrack != null ) {
				mCurrentChannel = mNoiseManager.AudioPlayer.OpenFile( mCurrentTrack.File );

				mOpenTracks.Add( mCurrentChannel, mCurrentTrack );
				mNoiseManager.AudioPlayer.Play( mCurrentChannel );

				mPlayCommand.RaiseCanExecuteChanged();
				mPauseCommand.RaiseCanExecuteChanged();
			}
		}

		private void OnPlay( object sender ) {
			mNoiseManager.AudioPlayer.Play( mCurrentChannel );

			NotifyOfPropertyChange( () => TrackPosition );
		}
		private bool CanPlay( object sender ) {
			return( mCurrentTrack != null );
		}
		public ICommand PlayCommand {
			get{ return( mPlayCommand ); }
		}

		private void OnPause( object sender ) {
			mNoiseManager.AudioPlayer.Pause( mCurrentChannel );

			NotifyOfPropertyChange( () => TrackPosition );
		}
		private bool CanPause( object sender ) {
			return( mCurrentTrack != null );
		}
		public ICommand PauseCommand {
			get{ return( mPauseCommand ); }
		}
	}
}
