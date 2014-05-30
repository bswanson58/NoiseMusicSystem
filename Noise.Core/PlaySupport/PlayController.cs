using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Timers;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Stateless;

namespace Noise.Core.PlaySupport {
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
									IHandle<Events.PlayQueueChanged>, IHandle<Events.PlayQueuedTrackRequest>,
									IHandle<Events.SystemShutdown>, IHandle<Events.GlobalUserEvent> {
		private readonly IEventAggregator		mEventAggregator;
		private readonly IAudioPlayer			mAudioPlayer;
		private readonly IPlayQueue				mPlayQueue;
		private readonly IPlayHistory			mPlayHistory;
		private readonly IScrobbler				mScrobbler;
		private readonly IPreferences			mPreferences;
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
		private readonly Subject<ePlayState>	mPlayStateSubject;
		private readonly Dictionary<int, PlayQueueTrack>	mOpenTracks;
		private StateMachine<ePlayState, eStateTriggers>	mPlayStateController;
		private ePlayState									mCurrentPlayState;

		public IObservable<ePlayState>			PlayStateChange { get { return ( mPlayStateSubject.AsObservable()); } }

		public PlayController( ILifecycleManager lifecycleManager, IEventAggregator eventAggregator,
							   IPlayQueue playQueue, IPlayHistory playHistory, IScrobbler scrobbler,
							   IAudioPlayer audioPlayer, IPreferences preferences ) {
			mEventAggregator = eventAggregator;
			mPlayQueue = playQueue;
			mPlayHistory = playHistory;
			mScrobbler = scrobbler;
			mAudioPlayer = audioPlayer;
			mPreferences = preferences;

			lifecycleManager.RegisterForInitialize( this );

			mOpenTracks = new Dictionary<int, PlayQueueTrack>();

			mInfoUpdateTimer = new Timer { AutoReset = true, Enabled = false, Interval = 250 };
			mInfoUpdateTimer.Elapsed += OnInfoUpdateTimer;

			mPlayStateSubject = new Subject<ePlayState>();
			PlayState = ePlayState.StoppedEmptyQueue;

			NoiseLogger.Current.LogInfo( "PlayController created" );
		}

		public void Initialize() {
			try {
				mSampleLevels = new AudioLevels();
				mCurrentPosition = new TimeSpan();
				mCurrentLength = new TimeSpan( 1 );

				mPlayStatusDispose = mAudioPlayer.ChannelStatusChange.Subscribe( OnPlayStatusChanged );
				mAudioLevelsDispose = mAudioPlayer.AudioLevelsChange.Subscribe( OnAudioLevelsChanged );
				mStreamInfoDispose = mAudioPlayer.AudioStreamInfoChange.Subscribe( OnStreamInfo );

				mEventAggregator.Subscribe( this );


				var preferences = mPreferences.Load<NoiseCorePreferences>();
				mDisplayTimeElapsed = preferences.DisplayPlayTimeElapsed;

				var audioCongfiguration = mPreferences.Load<AudioPreferences>();
				if( audioCongfiguration != null ) {
					mEnableReplayGain = audioCongfiguration.ReplayGainEnabled;
				}

				mPlayStateController = new StateMachine<ePlayState, eStateTriggers>( () => PlayState, newState => PlayState = newState );
				CurrentStatus = ePlaybackStatus.Stopped;

				mPlayStateController.OnUnhandledTrigger( ( state, trigger ) => { } );

				mPlayStateController.Configure( ePlayState.StoppedEmptyQueue )
					.OnEntry( StopPlay )
					.Permit( eStateTriggers.QueueTrackAdded, ePlayState.StartPlaying );

				mPlayStateController.Configure( ePlayState.Stopping )
					.OnEntry( StopPlay )
					.Permit( eStateTriggers.PlayerStopped, ePlayState.Stopped )
					.Permit( eStateTriggers.QueueCleared, ePlayState.StoppedEmptyQueue )
					.Permit( eStateTriggers.ExternalPlay, ePlayState.ExternalPlay );

				mPlayStateController.Configure( ePlayState.Stopped )
					.Permit( eStateTriggers.UiPlay, ePlayState.StartPlaying )
					.Permit( eStateTriggers.QueueCleared, ePlayState.StoppedEmptyQueue )
					.Permit( eStateTriggers.ExternalPlay, ePlayState.ExternalPlay );

				mPlayStateController.Configure( ePlayState.StartPlaying )
					.OnEntry( StartPlaying )
					.Permit( eStateTriggers.PlayerPlaying, ePlayState.Playing )
					.Permit( eStateTriggers.QueueCleared, ePlayState.StoppedEmptyQueue )
					.Permit( eStateTriggers.QueueExhausted, ePlayState.Stopped )
					.Permit( eStateTriggers.ExternalPlay, ePlayState.ExternalPlay );

				mPlayStateController.Configure( ePlayState.Playing )
					.Permit( eStateTriggers.PlayerStopped, ePlayState.PlayNext )
					.Permit( eStateTriggers.UiStop, ePlayState.Stopping )
					.Permit( eStateTriggers.UiPause, ePlayState.Pausing )
					.Permit( eStateTriggers.UiPlayNext, ePlayState.PlayNext )
					.Permit( eStateTriggers.UiPlayPrevious, ePlayState.PlayPrevious )
					.Permit( eStateTriggers.QueueCleared, ePlayState.StoppedEmptyQueue )
					.Permit( eStateTriggers.ExternalPlay, ePlayState.ExternalPlay );

				mPlayStateController.Configure( ePlayState.PlayNext )
					.OnEntry( PlayNext )
					.PermitReentry( eStateTriggers.UiPlayNext )
					.Permit( eStateTriggers.PlayerPlaying, ePlayState.Playing )
					.Permit( eStateTriggers.QueueExhausted, ePlayState.Stopped )
					.Permit( eStateTriggers.ExternalPlay, ePlayState.ExternalPlay );

				mPlayStateController.Configure( ePlayState.Pausing )
					.OnEntry( PausePlay )
					.PermitReentry( eStateTriggers.PlayerPlaying )
					.Permit( eStateTriggers.PlayerPaused, ePlayState.Paused )
					.Permit( eStateTriggers.PlayerPlaying, ePlayState.Playing )
					.Permit( eStateTriggers.ExternalPlay, ePlayState.ExternalPlay );

				mPlayStateController.Configure( ePlayState.Paused )
					.Permit( eStateTriggers.PlayerPlaying, ePlayState.Pausing )
					.Permit( eStateTriggers.PlayerStopped, ePlayState.Stopped )
					.Permit( eStateTriggers.QueueCleared, ePlayState.StoppedEmptyQueue )
					.Permit( eStateTriggers.ExternalPlay, ePlayState.ExternalPlay )
					.Permit( eStateTriggers.UiPlay, ePlayState.Resuming )
					.Permit( eStateTriggers.UiPlayNext, ePlayState.PlayNext )
					.Permit( eStateTriggers.UiPlayPrevious, ePlayState.PlayPrevious )
					.Permit( eStateTriggers.UiStop, ePlayState.Stopping );

				mPlayStateController.Configure( ePlayState.Resuming )
					.OnEntry( ResumePlay )
					.Permit( eStateTriggers.PlayerPlaying, ePlayState.Playing )
					.Permit( eStateTriggers.QueueCleared, ePlayState.StoppedEmptyQueue )
					.Permit( eStateTriggers.ExternalPlay, ePlayState.ExternalPlay )
					.Permit( eStateTriggers.UiPause, ePlayState.Pausing )
					.Permit( eStateTriggers.UiPlayNext, ePlayState.PlayNext )
					.Permit( eStateTriggers.UiPlayPrevious, ePlayState.PlayPrevious )
					.Permit( eStateTriggers.UiStop, ePlayState.Stopping );

				mPlayStateController.Configure( ePlayState.PlayPrevious )
					.OnEntry( PlayPrevious )
					.PermitReentry( eStateTriggers.UiPlayPrevious )
					.Permit( eStateTriggers.PlayerPlaying, ePlayState.Playing )
					.Permit( eStateTriggers.QueueExhausted, ePlayState.Stopped )
					.Permit( eStateTriggers.ExternalPlay, ePlayState.ExternalPlay );

				mPlayStateController.Configure( ePlayState.ExternalPlay )
					.Permit( eStateTriggers.PlayerPlaying, ePlayState.Playing )
					.Permit( eStateTriggers.QueueExhausted, ePlayState.Stopped );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "PlayController:Initialize", ex );
			}
		}

		public void Shutdown() { }

		public ePlayState PlayState {
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
			   (!mPlayQueue.CanPlayNextTrack())) {
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
			var circuitBreaker = 3;

			while(( track != null ) &&
				  ( circuitBreaker > 0 ) &&
				  (!StartTrack( track ))) {
				if( track.IsStrategyQueued ) {
					circuitBreaker--;

					if( circuitBreaker == 0 ) {
						FireStateChange( eStateTriggers.QueueExhausted );

						CurrentStatus = ePlaybackStatus.Stopped;
					}
				}

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

				mEventAggregator.Publish( new Events.PlaybackStatusChanged( mCurrentStatus ));
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - PlayController:StateChange: ", ex );
			}
		}

		private void OnPlayStatusChanged( AudioChannelStatus args ) {
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

		public void Handle( Events.SystemShutdown eventArgs ) {
			FireStateChange( eStateTriggers.QueueCleared );
			StopAllTracks( true );

			mInfoUpdateTimer.Stop();
			mEventAggregator.Unsubscribe( this );
			mAudioLevelsDispose.Dispose();
			mPlayStatusDispose.Dispose();
			mStreamInfoDispose.Dispose();

			var audioCongfiguration = mPreferences.Load<AudioPreferences>();
			if( audioCongfiguration != null ) {
				audioCongfiguration.ReplayGainEnabled = mEnableReplayGain;

				mPreferences.Save( audioCongfiguration );
			}			
		}

		public ePlaybackStatus CurrentStatus {
			get { return( mCurrentStatus ); }
			set {
				if( mCurrentStatus != value ) {
					mCurrentStatus = value;

					mEventAggregator.Publish( new Events.PlaybackStatusChanged( mCurrentStatus ));
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
				mEventAggregator.Publish( new Events.PlaybackTrackStarted( track ));
				GlobalCommands.RequestLyrics.Execute( new LyricsRequestArgs( track.Artist, track.Track ));
			}
		}

		private void OnTrackEnded( int channel ) {
			var track = GetTrack( channel );

			if( track != null ) {
				track.PercentPlayed = mAudioPlayer.GetPercentPlayed( channel );
				mPlayHistory.TrackPlayCompleted( track );
				mScrobbler.TrackPlayed( track );
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

		public void Handle( Events.PlayQueuedTrackRequest eventArgs ) {
			FireStateChange( eStateTriggers.ExternalPlay );

			if(!StartTrack( eventArgs.QueuedTrack )) {
				FireStateChange( eStateTriggers.QueueExhausted );

				CurrentStatus = ePlaybackStatus.Stopped;
			}

			mPlayQueue.PlayingTrack = eventArgs.QueuedTrack;

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

				mScrobbler.TrackStarted( track );
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

			CurrentStatus = ePlaybackStatus.Stopped;
			FirePlaybackTrackChanged();
			FirePlaybackInfoChange();
		}

		private int OpenTrack( PlayQueueTrack track ) {
			var retValue = 0;

			if( track != null ) {
				retValue = track.IsStream ? mAudioPlayer.OpenStream( track.Stream ) : 
											mAudioPlayer.OpenFile( track.FilePath, DetermineReplayGain( track ));

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

		private float DetermineReplayGain( PlayQueueTrack track ) {
			var retValue = 0.0f;

			if(( mEnableReplayGain ) &&
			   ( !track.IsStream ) &&
			   ( track.Track != null )) {
				var useAlbumGain = false;
				var adjacentTrack = FindPreviousTrack( track );

				if( track.Album != null ) {
					if(( adjacentTrack != null ) &&
					   ( adjacentTrack.Album.DbId == track.Album.DbId )) {
						useAlbumGain = true;
					}
					else {
						adjacentTrack = FindNextTrack( track );

						if(( adjacentTrack != null ) &&
						   ( adjacentTrack.Album.DbId == track.Album.DbId )) {
							useAlbumGain = true;
						}
					}

				}

				retValue = useAlbumGain ? track.Track.ReplayGainAlbumGain : track.Track.ReplayGainTrackGain;
			}

			return ( retValue );
		}

		private PlayQueueTrack FindPreviousTrack( PlayQueueTrack track ) {
			var retValue = default( PlayQueueTrack );

			foreach( var queuedTrack in mPlayQueue.PlayList ) {
				if( queuedTrack.Uid == track.Uid ) {
					break;
				}

				retValue = queuedTrack;
			}

			return( retValue );
		}

		private PlayQueueTrack FindNextTrack( PlayQueueTrack track ) {
			var retValue = default( PlayQueueTrack );
			var foundTrack = false;

			foreach( var queuedTrack in mPlayQueue.PlayList ) {
				if( foundTrack ) {
					retValue = queuedTrack;

					break;
				}

				if( queuedTrack.Uid == track.Uid ) {
					foundTrack = true;
				}
			}

			return ( retValue );
		}

		private void OnStreamInfo( StreamInfo info ) {
			var track = GetTrack( info.Channel );

			if( track != null ) {
				track.StreamInfo = info;

				FirePlaybackTrackChanged();
			}
		}

		public void Handle( Events.GlobalUserEvent eventArgs ) {
			switch( eventArgs.UserEvent.EventAction ) {
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
			get{ return(( mPlayStateController.CanFire( eStateTriggers.UiPlayNext )) &&
						( mPlayQueue.CanPlayNextTrack())); }
		}
 
		public void PlayPreviousTrack() {
			FireStateChange( eStateTriggers.UiPlayPrevious );
		}

		public bool CanPlayPreviousTrack {
			get {
				return(( mPlayQueue.CanPlayPreviousTrack()) &&
					   ( mPlayStateController.CanFire( eStateTriggers.UiPlayPrevious )) ||
					  (( CurrentTrack != null ) &&
				       ( mCurrentStatus != ePlaybackStatus.Stopped ) &&
				       ( mCurrentPosition > new TimeSpan( 0,0,5 ))));
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

			var preferences = mPreferences.Load<NoiseCorePreferences>();

			preferences.DisplayPlayTimeElapsed = mDisplayTimeElapsed;
			mPreferences.Save( preferences );

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

		public double PlayPositionPercentage {
			get {
				var retValue = 0.0D;

				if( mCurrentChannel != 0 ) {
					if(( mCurrentPosition.Ticks > 0 ) &&
					   ( mCurrentLength > mCurrentPosition )) {
						retValue = Math.Min( 1.0D, mCurrentPosition.TotalMilliseconds / mCurrentLength.TotalMilliseconds );
					}
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

		private IUserSettings GetChannelSettings( int channel ) {
			IUserSettings	retValue = null;
			var				currentTrack = GetTrack( channel );

			if( currentTrack != null ) {
				retValue = currentTrack.Track ?? currentTrack.Stream as IUserSettings;
			}

			return( retValue );
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

		public BitmapSource GetSpectrumImage( int height, int width, Color baseColor, Color peakColor, Color peakHoldColor ) {
			return ( mAudioPlayer.GetSpectrumImage( mCurrentChannel, height, width, baseColor, peakColor, peakHoldColor ));
		}

		public bool ReplayGainEnable {
			get{ return( mEnableReplayGain ); }
			set { mEnableReplayGain = value; }
		}

		private void FirePlaybackInfoChange() {
			mEventAggregator.Publish( new Events.PlaybackInfoChanged());
		}

		private void FirePlaybackTrackChanged() {
			mEventAggregator.Publish( new Events.PlaybackTrackChanged());
		}
	}
}
