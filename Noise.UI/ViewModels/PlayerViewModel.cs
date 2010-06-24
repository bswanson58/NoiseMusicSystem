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
		private	readonly IUnityContainer		mContainer;
		private readonly IEventAggregator		mEvents;
		private readonly INoiseManager			mNoiseManager;
		private int								mCurrentChannel;
		private ePlayingChannelStatus			mCurrentStatus;
		private	bool							mContinuePlaying;
		private readonly Timer					mInfoUpdateTimer;
		private readonly Dictionary<int, PlayQueueTrack>	mOpenTracks;

		private readonly DelegateCommand<object>	mPlayCommand;
		private readonly DelegateCommand<object>	mPauseCommand;
		private readonly DelegateCommand<object>	mStopCommand;
		private readonly DelegateCommand<object>	mNextTrackCommand;
		private readonly DelegateCommand<object>	mPrevTrackCommand;
		private readonly DelegateCommand<object>	mClearQueueCommand;

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
			mPrevTrackCommand = new DelegateCommand<object>( OnPreviousTrack, CanPreviousTrack );
			mClearQueueCommand = new DelegateCommand<object>( OnClearQueue, CanClearQueue );

			mInfoUpdateTimer = new Timer { AutoReset = true, Enabled = false, Interval = 250 };
			mInfoUpdateTimer.Elapsed += OnInfoUpdateTimer;
			mCurrentStatus = ePlayingChannelStatus.Unknown;
		}

		public string TrackName {
			get {
				var retValue = "None";

				if( CurrentTrack != null ) {
					retValue = CurrentTrack.Track.Name;
				}

				return( retValue );
			} 
		}

		public TimeSpan TrackPosition {
			get {
				var	retValue = new TimeSpan();

				if( CurrentChannel != 0 ) {
					retValue = mNoiseManager.AudioPlayer.GetPlayPosition( CurrentChannel );
				}

				return( retValue );
			}
		}

		private ePlayingChannelStatus CurrentStatus {
			get{ return( mCurrentStatus ); }
			set {
				mCurrentStatus = value;

				mPlayCommand.RaiseCanExecuteChanged();
				mPauseCommand.RaiseCanExecuteChanged();
				mStopCommand.RaiseCanExecuteChanged();
			}
		}

		private PlayQueueTrack CurrentTrack {
			get { return( GetTrack( CurrentChannel )); }
		}

		private int CurrentChannel {
			get{ return( mCurrentChannel ); }
			set {
				mCurrentChannel = value;

				NotifyOfPropertyChange( () => TrackPosition );
				NotifyOfPropertyChange( () => TrackName );
			} 
		}

		private PlayQueueTrack GetTrack( int channel ) {
			PlayQueueTrack	retValue = null;

			if( mOpenTracks.ContainsKey( channel )) {
				retValue = mOpenTracks[channel];
			}

			return( retValue );
		}


		public void OnTrackSelected( DbTrack track ) {
			mNoiseManager.PlayQueue.Add( track );
		}

		public void OnPlayQueueChanged( IPlayQueue playQueue ) {
			if( playQueue.IsQueueEmpty ) {
				StopPlaying();
			}
			else {
				if( mOpenTracks.Count == 0 ) {
					StartPlaying();
				}
			}

			mNextTrackCommand.RaiseCanExecuteChanged();
			mPrevTrackCommand.RaiseCanExecuteChanged();
			mClearQueueCommand.RaiseCanExecuteChanged();
		}

		public void OnPlayStatusChanged( int channel ) {
			var status = mNoiseManager.AudioPlayer.GetChannelStatus( channel );
			if( channel == CurrentChannel ) {
				CurrentStatus = status;
			}
			else {
				if( CurrentChannel != 0 ) {
					CurrentStatus = mNoiseManager.AudioPlayer.GetChannelStatus( CurrentChannel );
				}
			}

			var track = GetTrack( channel );
			if( track != null ) {
				if( status == ePlayingChannelStatus.Stopped ) {
					track.PercentPlayed = mNoiseManager.AudioPlayer.GetPercentPlayed( channel );
					mNoiseManager.PlayQueue.TrackPlayCompleted( track );
					mNoiseManager.AudioPlayer.CloseFile( channel );
					mOpenTracks.Remove( channel );

					if( channel == CurrentChannel ) {
						CurrentChannel = 0;

						StopInfoUpdate();

						if( mContinuePlaying ) {
							StartNextPlaying();
						}
					}
				}
			}
		}

		private void StartPlaying() {
			if( CurrentStatus == ePlayingChannelStatus.Paused ) {
				mNoiseManager.AudioPlayer.Play( CurrentChannel );
			}
			else {
				StartTrack( mNoiseManager.PlayQueue.PlayingTrack ?? mNoiseManager.PlayQueue.PlayNextTrack());
			}
		}

		private void StartNextPlaying() {
			StartTrack( mNoiseManager.PlayQueue.PlayNextTrack());
		}

		private void StartPreviousPlaying() {
			StartTrack( mNoiseManager.PlayQueue.PlayPreviousTrack());
		}

		private void StartTrack( PlayQueueTrack track ) {
			if( CurrentTrack != null ) {
				mNoiseManager.AudioPlayer.FadeAndStop( CurrentChannel );
			}

			if( track != null ) {
				var	channel = mNoiseManager.AudioPlayer.OpenFile( track.File );

				mOpenTracks.Add( channel, track );
				CurrentChannel = channel;
				mNoiseManager.AudioPlayer.Play( CurrentChannel );

				StartInfoUpdate();
				mContinuePlaying = true;
			}

			NotifyOfPropertyChange( () => TrackName );
			mNextTrackCommand.RaiseCanExecuteChanged();
			mPrevTrackCommand.RaiseCanExecuteChanged();
		}

		private void PausePlaying() {
			if( CurrentTrack != null ) {
				mNoiseManager.AudioPlayer.FadeAndPause( CurrentChannel );
			}
		}

		private void StopPlaying() {
			mContinuePlaying = false;

			foreach( int channel in mOpenTracks.Keys ) {
				mNoiseManager.AudioPlayer.FadeAndStop( channel );
			}
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
			StartPlaying();
		}
		private bool CanPlay( object sender ) {
			return((!mNoiseManager.PlayQueue.IsQueueEmpty ) && ( CurrentStatus != ePlayingChannelStatus.Playing ));
		}
		public ICommand PlayCommand {
			get{ return( mPlayCommand ); }
		}

		private void OnPause( object sender ) {
			PausePlaying();
		}
		private bool CanPause( object sender ) {
			return(( CurrentTrack != null ) && ( CurrentStatus == ePlayingChannelStatus.Playing ));
		}
		public ICommand PauseCommand {
			get{ return( mPauseCommand ); }
		}

		private void OnStop( object sender ) {
			StopPlaying();
		}
		private bool CanStop( object sender ) {
			return( CurrentStatus == ePlayingChannelStatus.Paused || CurrentStatus == ePlayingChannelStatus.Playing );
		}
		public ICommand StopCommand {
			get{ return( mStopCommand ); }
		}

		private void OnNextTrack( object sender ) {
			StartNextPlaying();
		}
		private bool CanNextTrack( object sender ) {
			return( mNoiseManager.PlayQueue.NextTrack != null );
		}
		public ICommand NextTrackCommand {
			get{ return( mNextTrackCommand ); }
		}

		private void OnPreviousTrack( object sender ) {
			StartPreviousPlaying();
		}
		private bool CanPreviousTrack( object sender ) {
			return( mNoiseManager.PlayQueue.PreviousTrack != null );
		}
		public ICommand PreviousTrackCommand {
			get{ return( mPrevTrackCommand ); }
		}

		private void OnClearQueue( object sender ) {
			mNoiseManager.PlayQueue.ClearQueue();
		}
		private bool CanClearQueue( object sender ) {
			return(!mNoiseManager.PlayQueue.IsQueueEmpty );
		}
		public ICommand ClearQueueCommand {
			get{ return( mClearQueueCommand ); }
		}
	}
}
