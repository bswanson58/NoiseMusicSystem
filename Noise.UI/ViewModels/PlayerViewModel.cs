using System;
using System.Collections.Generic;
using System.Timers;
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
		private ePlayingChannelStatus		mCurrentStatus;
		private readonly Timer				mInfoUpdateTimer;
		private readonly Dictionary<int, PlayQueueTrack>	mOpenTracks;

		private readonly DelegateCommand<object>	mPlayCommand;
		private readonly DelegateCommand<object>	mPauseCommand;
		private readonly DelegateCommand<object>	mNextTrackCommand;
		private readonly DelegateCommand<object>	mStopCommand;

		public PlayerViewModel( IUnityContainer container ) {
			mContainer = container;
			mEvents = mContainer.Resolve<IEventAggregator>();
			mNoiseManager = mContainer.Resolve<INoiseManager>();
			mOpenTracks = new Dictionary<int, PlayQueueTrack>();

			mEvents.GetEvent<Events.TrackSelected>().Subscribe( OnTrackSelected );
			mEvents.GetEvent<Events.PlayQueueChanged>().Subscribe( OnPlayQueueChanged );
			mEvents.GetEvent<Events.AudioPlayStatusChanged>().Subscribe( OnPlayStatusChanged );

			mPlayCommand = new DelegateCommand<object>( OnPlay, CanPlay );
			mPauseCommand = new DelegateCommand<object>( OnPause, CanPause );
			mStopCommand = new DelegateCommand<object>( OnStop, CanStop );
			mNextTrackCommand = new DelegateCommand<object>( OnNextTrack, CanNextTrack );

			mInfoUpdateTimer = new Timer { AutoReset = true, Enabled = false, Interval = 250 };
			mInfoUpdateTimer.Elapsed += OnInfoUpdateTimer;
			mCurrentStatus = ePlayingChannelStatus.Unknown;
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
					retValue = mNoiseManager.AudioPlayer.GetPlayPosition( mCurrentChannel );
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

			mNextTrackCommand.RaiseCanExecuteChanged();
		}

		public void OnPlayStatusChanged( int channel ) {
			if( mOpenTracks.ContainsKey( channel )) {
				if( channel == mCurrentChannel ) {
					mCurrentStatus = mNoiseManager.AudioPlayer.GetChannelStatus( channel );

					if( mCurrentStatus == ePlayingChannelStatus.Stopped ) {
						mNoiseManager.AudioPlayer.CloseFile( channel );

						mCurrentTrack = null;
						mCurrentChannel = 0;

						StopInfoUpdate();
						StartPlaying();

						NotifyOfPropertyChange( () => TrackName );
					}

					mPlayCommand.RaiseCanExecuteChanged();
					mPauseCommand.RaiseCanExecuteChanged();
				}
			}
		}

		private void StartPlaying() {
			mCurrentTrack = mNoiseManager.PlayQueue.PlayNextTrack();
			if( mCurrentTrack != null ) {
				mCurrentChannel = mNoiseManager.AudioPlayer.OpenFile( mCurrentTrack.File );

				mOpenTracks.Add( mCurrentChannel, mCurrentTrack );
				mNoiseManager.AudioPlayer.Play( mCurrentChannel );

				StartInfoUpdate();
			}

			mNextTrackCommand.RaiseCanExecuteChanged();
			NotifyOfPropertyChange( () => TrackName );
		}

		private void NextTrack() {
			if( mCurrentTrack != null ) {
				mNoiseManager.AudioPlayer.FadeAndStop( mCurrentChannel );
			}

			StartPlaying();
		}

		private void OnInfoUpdateTimer( object sender, ElapsedEventArgs arg ) {
			NotifyOfPropertyChange( () => TrackPosition );
		}

		private void StartInfoUpdate() {
			mInfoUpdateTimer.Start();
		}

		private void StopInfoUpdate() {
			mInfoUpdateTimer.Stop();
		}

		private void OnPlay( object sender ) {
			mNoiseManager.AudioPlayer.Play( mCurrentChannel );

			NotifyOfPropertyChange( () => TrackPosition );
		}
		private bool CanPlay( object sender ) {
			return(( mCurrentTrack != null ) && ( mCurrentStatus != ePlayingChannelStatus.Playing ));
		}
		public ICommand PlayCommand {
			get{ return( mPlayCommand ); }
		}

		private void OnPause( object sender ) {
			mNoiseManager.AudioPlayer.FadeAndPause( mCurrentChannel );

			NotifyOfPropertyChange( () => TrackPosition );
		}
		private bool CanPause( object sender ) {
			return(( mCurrentTrack != null ) && ( mCurrentStatus == ePlayingChannelStatus.Playing ));
		}
		public ICommand PauseCommand {
			get{ return( mPauseCommand ); }
		}

		private void OnStop( object sender ) {
			if( mCurrentTrack != null ) {
				mNoiseManager.AudioPlayer.FadeAndStop( mCurrentChannel );
			}
		}
		private bool CanStop( object sender ) {
			return(( mCurrentTrack != null ) && ( mCurrentStatus == ePlayingChannelStatus.Paused || mCurrentStatus == ePlayingChannelStatus.Playing ));
		}
		public ICommand StopCommand {
			get{ return( mStopCommand ); }
		}

		private void OnNextTrack( object sender ) {
			NextTrack();
		}
		private bool CanNextTrack( object sender ) {
			return( mNoiseManager.PlayQueue.NextTrack != null );
		}
		public ICommand NextTrackCommand {
			get{ return( mNextTrackCommand ); }
		}
	}
}
