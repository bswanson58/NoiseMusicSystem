using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using Microsoft.Practices.Prism.Events;
using Noise.Core.Support;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Stateless;

namespace Noise.Core.MediaPlayer {
	internal enum eStateTriggers {
		UiPlay,
		UiStop,
		UiPause,
		UiPlayNext,
		UiPlayPrevious,
		QueueTrackAdded,
		QueueExhausted,
		QueueCleared,
		PlayerStopped,
		PlayerPaused,
		PlayerPlaying,
		ExternalPlay
	}

	internal class PlayController : IPlayController, IRequireInitialization,
									IHandle<Events.PlayQueueChanged> {
		private readonly IEventAggregator		mEvents;
		private readonly ICaliburnEventAggregator	mEventAggregator;
		private readonly IAudioPlayer			mAudioPlayer;
		private	readonly IEqManager				mEqManager;
		private readonly IPlayQueue				mPlayQueue;
		private readonly IPlayHistory			mPlayHistory;
		private TimeSpan						mCurrentPosition;
		private TimeSpan						mCurrentLength;
		private bool							mDisplayTimeElapsed;
		private AudioLevels						mSampleLevels;
		private readonly Timer					mInfoUpdateTimer;
		private int								mCurrentChannel;
		private ePlaybackStatus					mCurrentStatus;
		private bool							mEnableReplayGain;
		private IDisposable						mPlayStatusDispose;
		private IDisposable						mAudioLevelsDispose;
		private IDisposable						mStreamInfoDispose;
		private readonly Dictionary<int, PlayQueueTrack>	mOpenTracks;
		private StateMachine<ePlayState, eStateTriggers>	mPlayStateController;
		private ePlayState									mCurrentPlayState;

		private Subject<ePlayState>				mPlayStateSubject;
		public	IObservable<ePlayState>			PlayStateChange { get { return( mPlayStateSubject.AsObservable()); } }

		public PlayController( ILifecycleManager lifecycleManager, IEventAggregator eventAggregator, ICaliburnEventAggregator caliburnEventAggregator,
							   IPlayQueue playQueue, IPlayHistory playHistory, IAudioPlayer audioPlayer, IEqManager eqManager ) {
			mEvents = eventAggregator;
			mEventAggregator = caliburnEventAggregator;
			mPlayQueue = playQueue;
			mPlayHistory = playHistory;
			mAudioPlayer = audioPlayer;
			mEqManager = eqManager;

			lifecycleManager.RegisterForInitialize( this );

			mOpenTracks = new Dictionary<int, PlayQueueTrack>();

			mInfoUpdateTimer = new Timer { AutoReset = true, Enabled = false, Interval = 250 };
			mInfoUpdateTimer.Elapsed += OnInfoUpdateTimer;

			NoiseLogger.Current.LogInfo( "PlayController created" );
		}

		public void Initialize() {
			mSampleLevels = new AudioLevels();
			mCurrentPosition = new TimeSpan();
			mCurrentLength = new TimeSpan( 1 );

			mPlayStatusDispose = mAudioPlayer.ChannelStatusChange.Subscribe( OnPlayStatusChanged );
			mAudioLevelsDispose = mAudioPlayer.AudioLevelsChange.Subscribe( OnAudioLevelsChanged );
			mStreamInfoDispose = mAudioPlayer.AudioStreamInfoChange.Subscribe( OnStreamInfo );

			mEventAggregator.Subscribe( this );
			mEvents.GetEvent<Events.DatabaseItemChanged>().Subscribe( OnDatabaseItemChanged );
			mEvents.GetEvent<Events.PlayRequested>().Subscribe( OnPlayRequested );
			mEvents.GetEvent<Events.GlobalUserEvent>().Subscribe( OnGlobalRequest );
			mEvents.GetEvent<Events.SystemShutdown>().Subscribe( OnShutdown );

			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

			if( configuration != null ) {
				mDisplayTimeElapsed = configuration.DisplayPlayTimeElapsed;
			}

			if( mEqManager.Initialize( Constants.EqPresetsFile )) {
				mAudioPlayer.ParametricEq = mEqManager.CurrentEq;
			}
			else {
				NoiseLogger.Current.LogMessage( "EqManager could not be initialized." );
			}

			var audioCongfiguration = NoiseSystemConfiguration.Current.RetrieveConfiguration<AudioConfiguration>( AudioConfiguration.SectionName );
			if( audioCongfiguration != null ) {
				mEnableReplayGain = audioCongfiguration.ReplayGainEnabled;
				mAudioPlayer.EqEnabled = audioCongfiguration.EqEnabled;
				mAudioPlayer.PreampVolume = audioCongfiguration.PreampGain;
				mAudioPlayer.StereoEnhancerEnable = audioCongfiguration.StereoEnhancerEnabled;
				mAudioPlayer.StereoEnhancerWidth = audioCongfiguration.StereoEnhancerWidth;
				mAudioPlayer.StereoEnhancerWetDry = audioCongfiguration.StereoEnhancerWetDry;
				mAudioPlayer.SoftSaturationEnable = audioCongfiguration.SoftSaturationEnabled;
				mAudioPlayer.SoftSaturationFactor = audioCongfiguration.SoftSaturationFactor;
				mAudioPlayer.SoftSaturationDepth = audioCongfiguration.SoftSaturationDepth;
				mAudioPlayer.ReverbEnable = audioCongfiguration.ReverbEnabled;
				mAudioPlayer.ReverbDelay = audioCongfiguration.ReverbDelay;
				mAudioPlayer.ReverbLevel = audioCongfiguration.ReverbLevel;
				mAudioPlayer.TrackOverlapEnable = audioCongfiguration.TrackOverlapEnabled;
				mAudioPlayer.TrackOverlapMilliseconds = audioCongfiguration.TrackOverlapMilliseconds;
			}

			mPlayStateSubject = new Subject<ePlayState>();
			PlayState = ePlayState.StoppedEmptyQueue;
			mPlayStateController = new StateMachine<ePlayState, eStateTriggers>( () => PlayState, newState => PlayState = newState );
			CurrentStatus = ePlaybackStatus.Stopped;

			mPlayStateController.Configure( ePlayState.StoppedEmptyQueue )
				.OnEntry( StopPlay )
				.Permit( eStateTriggers.QueueTrackAdded, ePlayState.StartPlaying )
				.Ignore( eStateTriggers.QueueCleared )
				.Ignore( eStateTriggers.PlayerStopped );

			mPlayStateController.Configure( ePlayState.Stopping )
				.OnEntry( StopPlay )
				.Permit( eStateTriggers.PlayerStopped, ePlayState.Stopped )
				.Permit( eStateTriggers.QueueCleared, ePlayState.StoppedEmptyQueue )
				.Permit( eStateTriggers.ExternalPlay, ePlayState.ExternalPlay )
				.Ignore( eStateTriggers.PlayerPaused )
				.Ignore( eStateTriggers.UiStop )
				.Ignore( eStateTriggers.UiPause )
				.Ignore( eStateTriggers.UiPlay )
				.Ignore( eStateTriggers.UiPlayNext )
				.Ignore( eStateTriggers.UiPlayPrevious )
				.Ignore( eStateTriggers.QueueTrackAdded );

			mPlayStateController.Configure( ePlayState.Stopped )
				.OnEntry( StopPlay )
				.Permit( eStateTriggers.UiPlay, ePlayState.StartPlaying )
				.Permit( eStateTriggers.QueueCleared, ePlayState.StoppedEmptyQueue )
				.Permit( eStateTriggers.ExternalPlay, ePlayState.ExternalPlay )
				.Ignore( eStateTriggers.PlayerStopped )
				.Ignore( eStateTriggers.QueueTrackAdded );

			mPlayStateController.Configure( ePlayState.StartPlaying )
				.OnEntry( StartPlaying )
				.Permit( eStateTriggers.PlayerPlaying, ePlayState.Playing )
				.Permit( eStateTriggers.QueueCleared, ePlayState.StoppedEmptyQueue )
				.Permit( eStateTriggers.QueueExhausted, ePlayState.Stopped )
				.Permit( eStateTriggers.ExternalPlay, ePlayState.ExternalPlay )
				.Ignore( eStateTriggers.QueueTrackAdded )
				.Ignore( eStateTriggers.PlayerPaused );

			mPlayStateController.Configure( ePlayState.Playing )
				.Permit( eStateTriggers.PlayerStopped, ePlayState.PlayNext )
				.Permit( eStateTriggers.UiStop, ePlayState.Stopping )
				.Permit( eStateTriggers.UiPause, ePlayState.Pausing )
				.Permit( eStateTriggers.UiPlayNext, ePlayState.PlayNext )
				.Permit( eStateTriggers.UiPlayPrevious, ePlayState.PlayPrevious )
				.Permit( eStateTriggers.QueueCleared, ePlayState.StoppedEmptyQueue )
				.Permit( eStateTriggers.ExternalPlay, ePlayState.ExternalPlay )
				.Ignore( eStateTriggers.QueueTrackAdded )
				.Ignore( eStateTriggers.PlayerPaused )
				.Ignore( eStateTriggers.PlayerPlaying );

			mPlayStateController.Configure( ePlayState.PlayNext )
				.OnEntry( PlayNext )
				.PermitReentry( eStateTriggers.UiPlayNext )
				.Permit( eStateTriggers.PlayerPlaying, ePlayState.Playing )
				.Permit( eStateTriggers.QueueExhausted, ePlayState.Stopped )
				.Permit( eStateTriggers.ExternalPlay, ePlayState.ExternalPlay )
				.Ignore( eStateTriggers.QueueTrackAdded )
				.Ignore( eStateTriggers.PlayerPaused )
				.Ignore( eStateTriggers.PlayerStopped );

			mPlayStateController.Configure( ePlayState.Pausing )
				.OnEntry( PausePlay )
				.PermitReentry( eStateTriggers.PlayerPlaying )
				.Permit( eStateTriggers.PlayerPaused, ePlayState.Paused )
				.Permit( eStateTriggers.PlayerPlaying, ePlayState.Playing )
				.Permit( eStateTriggers.ExternalPlay, ePlayState.ExternalPlay )
				.Ignore( eStateTriggers.UiStop )
				.Ignore( eStateTriggers.UiPause )
				.Ignore( eStateTriggers.UiPlay )
				.Ignore( eStateTriggers.UiPlayNext )
				.Ignore( eStateTriggers.UiPlayPrevious );

			mPlayStateController.Configure( ePlayState.Paused )
				.Permit( eStateTriggers.PlayerPlaying, ePlayState.Pausing )
				.Permit( eStateTriggers.PlayerStopped, ePlayState.Stopped )
				.Permit( eStateTriggers.QueueCleared, ePlayState.StoppedEmptyQueue )
				.Permit( eStateTriggers.ExternalPlay, ePlayState.ExternalPlay )
				.Permit( eStateTriggers.UiPlay, ePlayState.Resuming )
				.Permit( eStateTriggers.UiPlayNext, ePlayState.PlayNext )
				.Permit( eStateTriggers.UiPlayPrevious, ePlayState.PlayPrevious )
				.Permit( eStateTriggers.UiStop, ePlayState.Stopped )
				.Ignore( eStateTriggers.QueueTrackAdded );

			mPlayStateController.Configure( ePlayState.Resuming )
				.OnEntry( ResumePlay )
				.Permit( eStateTriggers.PlayerPlaying, ePlayState.Playing )
				.Permit( eStateTriggers.QueueCleared, ePlayState.StoppedEmptyQueue )
				.Permit( eStateTriggers.ExternalPlay, ePlayState.ExternalPlay )
				.Permit( eStateTriggers.UiPause, ePlayState.Pausing )
				.Permit( eStateTriggers.UiPlayNext, ePlayState.PlayNext )
				.Permit( eStateTriggers.UiPlayPrevious, ePlayState.PlayPrevious )
				.Permit( eStateTriggers.UiStop, ePlayState.Stopped );

			mPlayStateController.Configure( ePlayState.PlayPrevious )
				.OnEntry( PlayPrevious )
				.PermitReentry( eStateTriggers.UiPlayPrevious )
				.Permit( eStateTriggers.PlayerPlaying, ePlayState.Playing )
				.Permit( eStateTriggers.QueueExhausted, ePlayState.Stopped )
				.Permit( eStateTriggers.ExternalPlay, ePlayState.ExternalPlay )
				.Ignore( eStateTriggers.PlayerPaused )
				.Ignore( eStateTriggers.PlayerStopped );

			mPlayStateController.Configure( ePlayState.ExternalPlay )
				.Permit( eStateTriggers.PlayerPlaying, ePlayState.Playing )
				.Permit( eStateTriggers.QueueExhausted, ePlayState.Stopped )
				.Ignore( eStateTriggers.PlayerStopped );
		}

		public void Shutdown() { }

		private ePlayState PlayState {
			get{ return( mCurrentPlayState ); }
			set {
				mCurrentPlayState = value;
				mPlayStateSubject.OnNext( mCurrentPlayState );
			}
		}

		private void StopPlay() {
			StopAllTracks( false );
		}

		private void PausePlay() {
			foreach( var track in mOpenTracks.Keys ) {
				mAudioPlayer.FadeAndPause( track );
			}
		}

		private void ResumePlay() {
			mAudioPlayer.Play( mCurrentChannel );
		}

		private void StartPlaying() {
			if(( mPlayQueue.PlayingTrack == null ) &&
			   ( mPlayQueue.NextTrack == null )) {
				mPlayQueue.ReplayQueue();
			}

			PlayNext();
		}

		private void PlayNext() {
			PlayTrack( mPlayQueue.PlayNextTrack );
		}

		private void PlayPrevious() {
			if(( CurrentTrack != null ) &&
			   ( mCurrentStatus != ePlaybackStatus.Stopped ) &&
			   ( mCurrentPosition > new TimeSpan( 0, 0, 5 ))) {
				PlayPosition = 0;

				FireStateChange( eStateTriggers.PlayerPlaying );
			}
			else {
				PlayTrack( mPlayQueue.PlayPreviousTrack );
			}
		}

		private void PlayTrack( Func<PlayQueueTrack> nextTrack ) {
			var track = nextTrack();

			while(( track != null ) &&
				  (!StartTrack( track ))) {
				CurrentStatus = ePlaybackStatus.Unknown;

				track = nextTrack();
			}

			if( track == null ) {
				FireStateChange( eStateTriggers.QueueExhausted );

				CurrentStatus = ePlaybackStatus.Stopped;
			}
		}

		private void FireStateChange( eStateTriggers trigger ) {
			try {
				mPlayStateController.Fire( trigger );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - PlayController:StateChange: ", ex );
			}
		}

		private void OnPlayStatusChanged( ChannelStatusArgs args ) {
			switch( args.Status ) {
				case ePlaybackStatus.TrackStart:
					OnTrackStarted( args.Channel );
					FireStateChange( eStateTriggers.PlayerPlaying );
					break;

				case ePlaybackStatus.TrackEnd:
					OnTrackEnded( args.Channel );
					break;

				case ePlaybackStatus.RequestNext:
					QueueNextTrack();
					break;

				case ePlaybackStatus.Paused:
					FireStateChange( eStateTriggers.PlayerPaused );
					break;

				case ePlaybackStatus.Stopped:
					FireStateChange( eStateTriggers.PlayerStopped );
					break;
			}

			if( args.Channel == mCurrentChannel ) {
				CurrentStatus = args.Status;
			}
		}

		private void OnShutdown( object sender ) {
			FireStateChange( eStateTriggers.QueueCleared );
			StopAllTracks( true );

			mAudioLevelsDispose.Dispose();
			mPlayStatusDispose.Dispose();
			mStreamInfoDispose.Dispose();

			var audioCongfiguration = NoiseSystemConfiguration.Current.RetrieveConfiguration<AudioConfiguration>( AudioConfiguration.SectionName );
			if( audioCongfiguration != null ) {
				audioCongfiguration.PreampGain = mAudioPlayer.PreampVolume;
				audioCongfiguration.ReplayGainEnabled = mEnableReplayGain;

				audioCongfiguration.StereoEnhancerEnabled = mAudioPlayer.StereoEnhancerEnable;
				audioCongfiguration.StereoEnhancerWidth = mAudioPlayer.StereoEnhancerWidth;
				audioCongfiguration.StereoEnhancerWetDry = mAudioPlayer.StereoEnhancerWetDry;
				audioCongfiguration.SoftSaturationEnabled = mAudioPlayer.SoftSaturationEnable;
				audioCongfiguration.SoftSaturationFactor = mAudioPlayer.SoftSaturationFactor;
				audioCongfiguration.SoftSaturationDepth = mAudioPlayer.SoftSaturationDepth;
				audioCongfiguration.ReverbEnabled = mAudioPlayer.ReverbEnable;
				audioCongfiguration.ReverbDelay = mAudioPlayer.ReverbDelay;
				audioCongfiguration.ReverbLevel = mAudioPlayer.ReverbLevel;
				audioCongfiguration.TrackOverlapEnabled = mAudioPlayer.TrackOverlapEnable;
				audioCongfiguration.TrackOverlapMilliseconds = mAudioPlayer.TrackOverlapMilliseconds;

				NoiseSystemConfiguration.Current.Save( audioCongfiguration );
			}			

			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

			if( configuration != null ) {
				configuration.DisplayPlayTimeElapsed = mDisplayTimeElapsed;

				NoiseSystemConfiguration.Current.Save( configuration );
			}
		}

		public ePlaybackStatus CurrentStatus {
			get { return( mCurrentStatus ); }
			set {
				if( mCurrentStatus != value ) {
					mCurrentStatus = value;

					mEvents.GetEvent<Events.PlaybackStatusChanged>().Publish( mCurrentStatus );
				}
			}
		}

		private PlayQueueTrack GetTrack( int channel ) {
			PlayQueueTrack	retValue = null;

			if( mOpenTracks.ContainsKey( channel )) {
				retValue = mOpenTracks[channel];
			}

			return( retValue );
		}

		public void Handle( Events.PlayQueueChanged eventArgs ) {
			if( eventArgs.PlayQueue.IsQueueEmpty ) {
				FireStateChange( eStateTriggers.QueueCleared );

				CurrentStatus = ePlaybackStatus.Stopped;
			}
			else {
				if(( CurrentTrack != null ) &&
				   (!mPlayQueue.IsTrackQueued( CurrentTrack.Track ))) {
					FireStateChange( eStateTriggers.UiStop );
				}
				else {
					FireStateChange( eStateTriggers.QueueTrackAdded );
				}
			}
		}

		private void OnTrackStarted( int channel ) {
			mCurrentChannel = channel;
			mCurrentLength = mAudioPlayer.GetLength( channel );

			StartInfoUpdate();

			FirePlaybackTrackChanged();

			var track = GetTrack( channel );

			if( track != null ) {
				mEvents.GetEvent<Events.PlaybackTrackStarted>().Publish( track );
				GlobalCommands.RequestLyrics.Execute( new LyricsRequestArgs( track.Artist, track.Track ));
			}
		}

		private void OnTrackEnded( int channel ) {
			var track = GetTrack( channel );

			if( track != null ) {
				track.PercentPlayed = mAudioPlayer.GetPercentPlayed( channel );
				mPlayHistory.TrackPlayCompleted( track );
			}

			mOpenTracks.Remove( channel );
			mAudioPlayer.CloseChannel( channel );

			if( mCurrentChannel == channel ) {
				mCurrentChannel = 0;
				mCurrentPosition = new TimeSpan();
				mCurrentLength = new TimeSpan( 1 );

				FirePlaybackInfoChange();
			}

			if( mOpenTracks.Count == 0 ) {
				StopInfoUpdate();

				FireStateChange( eStateTriggers.PlayerStopped );
			}
		}

		private void OnPlayRequested( PlayQueueTrack track ) {
			FireStateChange( eStateTriggers.ExternalPlay );

			StartTrack( track );

			mPlayQueue.PlayingTrack = track;

			FirePlaybackTrackChanged();
		}

		private void QueueNextTrack() {
			var track =  mPlayQueue.PlayNextTrack();

			if( track != null ) {
				var channel = OpenTrack( track );

				if( channel != 0 ) {
					mAudioPlayer.QueueNextChannel( channel );
				}
			}
		}

		private bool StartTrack( PlayQueueTrack track ) {
			var retValue = false;

			if( CurrentTrack != null ) {
				mAudioPlayer.FadeAndStop( mCurrentChannel );
			}

			var	channel = OpenTrack( track );
			if( channel != 0 ) {
				mAudioPlayer.Play( channel );

				retValue = true;
			}

			return( retValue );
		}

		private void StopAllTracks( bool stopImmediate ) {
			var openChannels = new List<int>( mOpenTracks.Keys );

			foreach( var channel in openChannels ) {
				if( stopImmediate ) {
					mAudioPlayer.Stop( channel );
				}
				else {
					mAudioPlayer.FadeAndStop( channel );
				}
			}

			StopInfoUpdate();

			mPlayQueue.StopPlay();

			mCurrentPosition = new TimeSpan();
			mCurrentLength = new TimeSpan( 1 );

			FirePlaybackTrackChanged();
			FirePlaybackInfoChange();
		}

		private int OpenTrack( PlayQueueTrack track ) {
			var retValue = 0;

			if( track != null ) {
				var gain = 0.0f;
				
				if(( mEnableReplayGain ) &&
					(!track.IsStream ) &&
					( track.Track != null )) {
					gain = track.Track.ReplayGainAlbumGain != 0.0f ? track.Track.ReplayGainAlbumGain : track.Track.ReplayGainTrackGain;
				}

				retValue = track.IsStream ? mAudioPlayer.OpenStream( track.Stream ) : mAudioPlayer.OpenFile( track.FilePath, gain );
				if( retValue != 0 ) {
					mOpenTracks.Add( retValue, track );

					track.IsFaulted = false;
				}
				else {
					track.IsFaulted = true;
				}
			}

			return( retValue );
		}

		public void OnDatabaseItemChanged( DbItemChangedArgs args ) {
			var item = args.Item;

			if( item is DbTrack ) {
				var track = item as DbTrack;

				foreach( var queueItem in mOpenTracks.Values ) {
					if(( !queueItem.IsStream ) &&
					   ( queueItem.Track.DbId == track.DbId )) {
						queueItem.UpdateTrack( track );

						FirePlaybackTrackChanged();
						break;
					}
				}
			}
		}

		private void OnStreamInfo( StreamInfo info ) {
			var track = GetTrack( info.Channel );

			if( track != null ) {
				track.StreamInfo = info;

				FirePlaybackTrackChanged();
			}
		}

		private void OnGlobalRequest( GlobalUserEventArgs args ) {
			switch( args.EventAction ) {
				case UserEventAction.PausePlay:
					if( CanPause ) {
						Pause();
					}
					break;

				case UserEventAction.PlayNextTrack:
					if( CanPlayNextTrack ) {
						PlayNextTrack();
					}
					break;

				case UserEventAction.PlayPreviousTrack:
					if( CanPlayPreviousTrack ) {
						PlayPreviousTrack();
					}
					break;

				case UserEventAction.StartPlay:
					if( CanPlay ) {
						Play();
					}
					break;

				case UserEventAction.StopPlay:
					if( CanStop ) {
						Stop();
					}
					break;
			}
		}

		public void Play() {
			FireStateChange( eStateTriggers.UiPlay );
		}

		public bool CanPlay {
			get { return( mPlayStateController.CanFire( eStateTriggers.UiPlay )); }
		}

		public void Pause() {
			FireStateChange( eStateTriggers.UiPause );
		}

		public bool CanPause {
			get{ return( mPlayStateController.CanFire( eStateTriggers.UiPause )); }
		}

		public void Stop() {
			FireStateChange( eStateTriggers.UiStop );
		}

		public bool CanStop {
			get{ return( mPlayStateController.CanFire( eStateTriggers.UiStop )); }
		}

		public void PlayNextTrack() {
			FireStateChange( eStateTriggers.UiPlayNext );
		}

		public bool CanPlayNextTrack {
			get{ return( mPlayStateController.CanFire( eStateTriggers.UiPlayNext )); }
		}
 
		public void PlayPreviousTrack() {
			FireStateChange( eStateTriggers.UiPlayPrevious );
		}

		public bool CanPlayPreviousTrack {
			get {
				var retValue = false;

				if(( mPlayQueue.PreviousTrack != null ) ||
					(( CurrentTrack != null ) &&
					( mCurrentStatus != ePlaybackStatus.Stopped ) &&
					( mCurrentPosition > new TimeSpan( 0,0,5 )))) {
					retValue = true;
				}

				return( retValue );
			}
		}

		public PlayQueueTrack CurrentTrack {
			get { return( GetTrack( mCurrentChannel )); }
		}

		public PlayQueueTrack NextTrack {
			get{ return( mPlayQueue.NextTrack ); }
		}

		public PlayQueueTrack PreviousTrack {
			get{ return( mPlayQueue.PreviousTrack ); }
		}

		private void OnAudioLevelsChanged( AudioLevels levels ) {
			mSampleLevels = levels;
		}

		private void OnInfoUpdateTimer( object sender, ElapsedEventArgs arg ) {
			if( mCurrentChannel != 0 ) {
				mCurrentPosition = mAudioPlayer.GetPlayPosition( mCurrentChannel );
			}
			else {
				mSampleLevels = new AudioLevels( 0.0, 0.0 );
			}

			FirePlaybackInfoChange();
		}

		private void StartInfoUpdate() {
			mInfoUpdateTimer.Start();
		}

		private void StopInfoUpdate() {
			mInfoUpdateTimer.Stop();

			mSampleLevels = new AudioLevels( 0.0, 0.0 );
			mCurrentPosition = new TimeSpan();
			mCurrentLength = new TimeSpan( 1 );

			FirePlaybackInfoChange();
		}

		public void ToggleTimeDisplay() {
			mDisplayTimeElapsed = !mDisplayTimeElapsed;

			FirePlaybackInfoChange();
		}

		public TimeSpan TrackTime {
			get {
				var retValue = new TimeSpan();

				if( mCurrentChannel != 0 ) {
					if( mDisplayTimeElapsed ) {
						retValue = mCurrentPosition;
					}
					else {
						retValue = mCurrentPosition - mCurrentLength;
					}
				}

				if( mCurrentLength < mCurrentPosition ) {
					mCurrentLength = mCurrentPosition;
				}

				return( retValue );
			}
		}

		public long PlayPosition {
			get { return( mCurrentChannel != 0 ? 
								( mCurrentPosition.Ticks < mCurrentLength.Ticks ? mCurrentPosition.Ticks : mCurrentLength.Ticks ) : 0L ); }
			set {
				if( mCurrentChannel != 0 ) {
					mAudioPlayer.SetPlayPosition( mCurrentChannel, new TimeSpan( value ));
				}
			}
		}

		public long TrackEndPosition {
			get { return( mCurrentChannel != 0 ? mCurrentLength.Ticks : 1L ); }
		}

		public double Volume {
			get{ return( mAudioPlayer.Volume ); }
			set{
				mAudioPlayer.Volume = (float)value; 

				FirePlaybackInfoChange();
			}
		}

		public bool Mute {
			get{ return( mAudioPlayer.Mute ); }
			set{ 
				mAudioPlayer.Mute = value; 

				FirePlaybackInfoChange();
			}
		}

		public double PreampVolume {
			get{ return( mAudioPlayer.PreampVolume ); }
			set{ mAudioPlayer.PreampVolume = (float)value; }
		}

		public double PlaySpeed {
			get{ return( mAudioPlayer.PlaySpeed ); }
			set {
				mAudioPlayer.PlaySpeed = (float)value;

				FirePlaybackInfoChange();
			}
		}

		public double PanPosition {
			get{ return( mAudioPlayer.Pan ); }
			set {
				mAudioPlayer.Pan = (float)value;

				FirePlaybackInfoChange();
			}
		}

		private IUserSettings GetChannelSettings( int channel ) {
			IUserSettings	retValue = null;
			var				currentTrack = GetTrack( channel );

			if( currentTrack != null ) {
				retValue = currentTrack.Track ?? currentTrack.Stream as IUserSettings;
			}

			return( retValue );
		}

		public bool TrackOverlapEnable {
			get{ return( mAudioPlayer.TrackOverlapEnable ); }
			set{ mAudioPlayer.TrackOverlapEnable = value; }
		}

		public int TrackOverlapMilliseconds {
			get{ return( mAudioPlayer.TrackOverlapMilliseconds ); }
			set{ mAudioPlayer.TrackOverlapMilliseconds = value; }
		}

		public bool IsFavorite {
			get {
				var	retValue = false;
				var settings = GetChannelSettings( mCurrentChannel );

				if( settings != null ) {
					retValue = settings.IsFavorite;
				}

				return( retValue );
			}
			set {
				var settings = GetChannelSettings( mCurrentChannel );
				if( settings != null ) {
					var track = GetTrack( mCurrentChannel );

					settings.IsFavorite = value;

					if( track != null ) {
						GlobalCommands.SetFavorite.Execute( track.IsStream ? new SetFavoriteCommandArgs( track.Stream.DbId, value ) :
																			 new SetFavoriteCommandArgs( track.Track.DbId, value ));
					}
				}
			}
		}

		public Int16 Rating {
			get {
				Int16	retValue = 0;
				var		settings = GetChannelSettings( mCurrentChannel );

				if( settings != null ) {
					retValue = settings.Rating;
				}

				return( retValue );
			}
			set {
				var settings = GetChannelSettings( mCurrentChannel );

				if( settings != null ) {
					var track = GetTrack( mCurrentChannel );

					settings.Rating = value;

					if( track.Track != null ) {
						GlobalCommands.SetRating.Execute( track.IsStream ? new SetRatingCommandArgs( track.Stream.DbId, value ) :
																		   new SetRatingCommandArgs( track.Track.DbId, value ));
					}
				}
			}
		}

		public double LeftLevel {
			get { return( mCurrentChannel != 0 ? mSampleLevels.LeftLevel : 0.0 ); }
		}

		public double RightLevel {
			get { return( mCurrentChannel != 0 ? mSampleLevels.RightLevel : 0.0 ); }
		}

		public bool ReplayGainEnable {
			get{ return( mEnableReplayGain ); }
			set { mEnableReplayGain = value; }
		}

		public IEqManager EqManager {
			get{ return( mEqManager ); }
		}

		public ParametricEqualizer CurrentEq {
			get{ return( mAudioPlayer.ParametricEq ); }
			set {
				mAudioPlayer.ParametricEq = value;
				mEqManager.CurrentEq = value;
			}
		}

		public bool EqEnabled {
			get{ return( mAudioPlayer.EqEnabled ); }
			set{ mAudioPlayer.EqEnabled = value; }
		}

		public void SetEqValue( long bandId, float gain ) {
			mAudioPlayer.AdjustEq( bandId, gain );
		}

		public BitmapSource GetSpectrumImage( int height, int width, Color baseColor, Color peakColor, Color peakHoldColor ) {
			return( mAudioPlayer.GetSpectrumImage( mCurrentChannel, height, width, baseColor, peakColor, peakHoldColor ));
		}

		public bool StereoEnhancerEnable {
			get{ return( mAudioPlayer.StereoEnhancerEnable ); }
			set{ mAudioPlayer.StereoEnhancerEnable = value; }
		}

		public double StereoEnhancerWidth {
			get{ return( mAudioPlayer.StereoEnhancerWidth ); }
			set{ mAudioPlayer.StereoEnhancerWidth = value; }
		}

		public double StereoEnhancerWetDry {
			get{ return( mAudioPlayer.StereoEnhancerWetDry ); }
			set{ mAudioPlayer.StereoEnhancerWetDry = value; }
		}

		public bool SoftSaturationEnable {
			get{ return( mAudioPlayer.SoftSaturationEnable ); }
			set{ mAudioPlayer.SoftSaturationEnable = value; }
		}

		public double SoftSaturationDepth {
			get{ return( mAudioPlayer.SoftSaturationDepth ); }
			set{ mAudioPlayer.SoftSaturationDepth = value; }
		}

		public double SoftSaturationFactor {
			get{ return( mAudioPlayer.SoftSaturationFactor ); }
			set{ mAudioPlayer.SoftSaturationFactor = value; }
		}

		public bool ReverbEnable {
			get{ return( mAudioPlayer.ReverbEnable ); }
			set{ mAudioPlayer.ReverbEnable = value; }
		}

		public float ReverbLevel {
			get{ return( mAudioPlayer.ReverbLevel ); }
			set{ mAudioPlayer.ReverbLevel = value; }
		}

		public int ReverbDelay {
			get{ return( mAudioPlayer.ReverbDelay ); }
			set{ mAudioPlayer.ReverbDelay = value; }
		}

		private void FirePlaybackInfoChange() {
			mEvents.GetEvent<Events.PlaybackInfoChanged>().Publish( null );
		}

		private void FirePlaybackTrackChanged() {
			mEvents.GetEvent<Events.PlaybackTrackChanged>().Publish( null );
		}
	}
}
