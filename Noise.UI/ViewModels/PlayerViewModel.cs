using System;
using System.Collections.Generic;
using System.Timers;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.UI.ViewModels {
	public class PlayerViewModel : ViewModelBase {
		private	readonly IUnityContainer		mContainer;
		private readonly IEventAggregator		mEvents;
		private readonly INoiseManager			mNoiseManager;
		private TimeSpan						mCurrentPosition;
		private TimeSpan						mCurrentLength;
		private AudioLevels						mSampleLevels;
		private	bool							mContinuePlaying;
		private readonly Timer					mInfoUpdateTimer;
		private readonly Dictionary<int, PlayQueueTrack>	mOpenTracks;

		public PlayerViewModel( IUnityContainer container ) {
			mContainer = container;
			mEvents = mContainer.Resolve<IEventAggregator>();
			mNoiseManager = mContainer.Resolve<INoiseManager>();
			mOpenTracks = new Dictionary<int, PlayQueueTrack>();

			mEvents.GetEvent<Events.TrackSelected>().Subscribe( OnTrackSelected );
			mEvents.GetEvent<Events.PlayQueueChanged>().Subscribe( OnPlayQueueChanged );
			mEvents.GetEvent<Events.AudioPlayStatusChanged>().Subscribe( OnPlayStatusChanged );

			mInfoUpdateTimer = new Timer { AutoReset = true, Enabled = false, Interval = 100 };
			mInfoUpdateTimer.Elapsed += OnInfoUpdateTimer;

			if( IsInDesignMode ) {
				mCurrentPosition = new TimeSpan( 0, 1, 23 );
				mCurrentLength = new TimeSpan( 0, 2, 33 );
				mSampleLevels = new AudioLevels( 0.5, 0.6 );
			}
		}

		private ePlayingChannelStatus CurrentStatus {
			get { return( Get(() => CurrentStatus, ePlayingChannelStatus.Stopped ));  }
			set { Set(() => CurrentStatus, value ); }
		}

		private PlayQueueTrack CurrentTrack {
			get { return( GetTrack( CurrentChannel )); }
		}

		private int CurrentChannel {
			get{ return ( Get(() => CurrentChannel, 0 )); }
			set{ Set(() => CurrentChannel, value ); }
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

		private int PlayQueueChangedFlag {
			get{ return( Get( () => PlayQueueChangedFlag, 0 )); }
			set{ Set( () => PlayQueueChangedFlag, value  ); }
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

			PlayQueueChangedFlag++;
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
					mNoiseManager.PlayHistory.TrackPlayCompleted( track );
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

		private int StartTrackFlag {
			get{ return( Get( () => StartTrackFlag, 0 )); }
			set{ Set( () => StartTrackFlag, value  ); }
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

				mCurrentLength = mNoiseManager.AudioPlayer.GetLength( CurrentChannel );

				StartInfoUpdate();
				mContinuePlaying = true;
			}

			StartTrackFlag++;
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

		private int InfoUpdateFlag {
			get{ return( Get( () => InfoUpdateFlag, 0 ));  }
			set{ Set( () => InfoUpdateFlag, value ); }
		}

		private void OnInfoUpdateTimer( object sender, ElapsedEventArgs arg ) {
			if( CurrentChannel != 0 ) {
				mSampleLevels = mNoiseManager.AudioPlayer.GetSampleLevels( CurrentChannel );
				mCurrentPosition = mNoiseManager.AudioPlayer.GetPlayPosition( CurrentChannel );
			}
			else {
				mSampleLevels = new AudioLevels( 0.0, 0.0 );
			}

			InfoUpdateFlag++;
		}

		private void StartInfoUpdate() {
			mInfoUpdateTimer.Start();
		}

		private void StopInfoUpdate() {
			mInfoUpdateTimer.Stop();

			mSampleLevels = new AudioLevels( 0.0, 0.0 );
			mCurrentPosition = new TimeSpan();

			InfoUpdateFlag++;
		}

		[DependsUpon( "StartTrackFlag" )]
		public string TrackName {
			get {
				var retValue = "None";

				if( CurrentTrack != null ) {
					retValue = CurrentTrack.Track.Name;
				}

				return( retValue );
			} 
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public TimeSpan TrackPosition {
			get {
				var	retValue = new TimeSpan();

				if( CurrentChannel != 0 ) {
					retValue = mCurrentLength - mCurrentPosition;
				}

				return( retValue );
			}
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public double LeftLevel {
			get {
				var retValue = 0.0;

				if( CurrentChannel != 0 ) {
					retValue = mSampleLevels.LeftLevel;
				}

				return( retValue );
			}
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public double RightLevel {
			get {
				var retValue = 0.0;

				if( CurrentChannel != 0 ) {
					retValue = mSampleLevels.RightLevel;
				}

				return( retValue );
			}
		}

		public void Execute_Play( object sender ) {
			StartPlaying();
		}
		[DependsUpon( "CurrentStatus" )]
		public bool CanExecute_Play( object sender ) {
			return((!mNoiseManager.PlayQueue.IsQueueEmpty ) && ( CurrentStatus != ePlayingChannelStatus.Playing ));
		}

		public void Execute_Pause( object sender ) {
			PausePlaying();
		}
		[DependsUpon( "CurrentStatus" )]
		public bool CanExecute_Pause( object sender ) {
			return(( CurrentTrack != null ) && ( CurrentStatus == ePlayingChannelStatus.Playing ));
		}

		public void Execute_Stop( object sender ) {
			StopPlaying();
		}
		[DependsUpon( "CurrentStatus" )]
		public bool CanExecute_Stop( object sender ) {
			return( CurrentStatus == ePlayingChannelStatus.Paused || CurrentStatus == ePlayingChannelStatus.Playing );
		}

		public void Execute_NextTrack( object sender ) {
			StartNextPlaying();
		}
		[DependsUpon( "PlayQueueChangedFlag" )]
		[DependsUpon( "StartTrackFlag" )]
		public bool CanExecute_NextTrack( object sender ) {
			return( mNoiseManager.PlayQueue.NextTrack != null );
		}

		public void Execute_PreviousTrack( object sender ) {
			StartPreviousPlaying();
		}
		[DependsUpon( "PlayQueueChangedFlag" )]
		[DependsUpon( "StartTrackFlag" )]
		public bool CanExecute_PreviousTrack( object sender ) {
			return( mNoiseManager.PlayQueue.PreviousTrack != null );
		}

		public void Execute_ClearQueue( object sender ) {
			mNoiseManager.PlayQueue.ClearQueue();
		}
		[DependsUpon( "PlayQueueChangedFlag" )]
		public bool CanExecute_ClearQueue( object sender ) {
			return(!mNoiseManager.PlayQueue.IsQueueEmpty );
		}
	}
}
