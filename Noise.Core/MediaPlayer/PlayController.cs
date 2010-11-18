using System;
using System.Collections.Generic;
using System.Timers;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.MediaPlayer {
	internal class PlayController : IPlayController {
		private	readonly IUnityContainer	mContainer;
		private readonly IEventAggregator	mEvents;
		private readonly INoiseManager		mNoiseManager;
		private readonly IAudioPlayer		mAudioPlayer;
		private TimeSpan					mCurrentPosition;
		private TimeSpan					mCurrentLength;
		private bool						mDisplayTimeElapsed;
		private AudioLevels					mSampleLevels;
		private	bool						mContinuePlaying;
		private readonly Timer				mInfoUpdateTimer;
		private int							CurrentChannel { get; set; }
		private ePlaybackStatus				mCurrentStatus;
		private readonly Dictionary<int, PlayQueueTrack>	mOpenTracks;

		private readonly ParametricEqualizer	mEq;

		public PlayController( IUnityContainer container ) {
			mContainer = container;
			mEvents = mContainer.Resolve<IEventAggregator>();
			mAudioPlayer = mContainer.Resolve<IAudioPlayer>();
			mNoiseManager = mContainer.Resolve<INoiseManager>();

			mOpenTracks = new Dictionary<int, PlayQueueTrack>();

			mInfoUpdateTimer = new Timer { AutoReset = true, Enabled = false, Interval = 100 };
			mInfoUpdateTimer.Elapsed += OnInfoUpdateTimer;

			mEvents.GetEvent<Events.PlayQueueChanged>().Subscribe( OnPlayQueueChanged );
			mEvents.GetEvent<Events.AudioPlayStatusChanged>().Subscribe( OnPlayStatusChanged );
			mEvents.GetEvent<Events.AudioPlayStreamInfo>().Subscribe( OnStreamInfo );
			mEvents.GetEvent<Events.DatabaseItemChanged>().Subscribe( OnDatabaseItemChanged );
			mEvents.GetEvent<Events.PlayRequested>().Subscribe( OnPlayRequested );

			var	systemConfig = mContainer.Resolve<ISystemConfiguration>();
			var configuration = systemConfig.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

			if( configuration != null ) {
				mDisplayTimeElapsed = configuration.DisplayPlayTimeElapsed;
			}

			mEq = new ParametricEqualizer { Bandwidth = 18f, Name = "Unnamed" };
			mEq.AddBands( new [] { new ParametricBand( 25.0f ), new ParametricBand( 60.0f ),
								   new ParametricBand( 120.0f ), new ParametricBand( 250.0f ),
								   new ParametricBand( 500.0f ), new ParametricBand( 1000.0f ),
								   new ParametricBand( 2000.0f ), new ParametricBand( 4000.0f ),
								   new ParametricBand( 8000.0f ), new ParametricBand( 16000.0f ) });
			mAudioPlayer.ParametricEq = mEq;
		}

		public ParametricEqualizer ParametricEq {
			get{ return( mAudioPlayer.ParametricEq ); }
			set{ mAudioPlayer.ParametricEq = value; }
		}

		public bool EqEnabled {
			get{ return( mAudioPlayer.EqEnabled ); }
			set{ mAudioPlayer.EqEnabled = value; }
		}

		public void SetEqValue( long bandId, float gain ) {
			mAudioPlayer.AdjustEq( bandId, gain );
		}

		public PlayQueueTrack CurrentTrack {
			get { return( GetTrack( CurrentChannel )); }
		}

		public PlayQueueTrack NextTrack {
			get{ return( mNoiseManager.PlayQueue.NextTrack ); }
		}

		public PlayQueueTrack PreviousTrack {
			get{ return( mNoiseManager.PlayQueue.PreviousTrack ); }
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

		public void OnPlayQueueChanged( IPlayQueue playQueue ) {
			if( playQueue.IsQueueEmpty ) {
				Stop();
			}
			else {
				if( mOpenTracks.Count == 0 ) {
					Play();
				}
			}
		}

		private void OnPlayRequested( PlayQueueTrack track ) {
			StartTrack( track );

			mNoiseManager.PlayQueue.PlayingTrack = track;
		}

		public void OnPlayStatusChanged( int channel ) {
			var status = mAudioPlayer.GetChannelStatus( channel );
			if( channel == CurrentChannel ) {
				CurrentStatus = status;
			}
			else {
				if( CurrentChannel != 0 ) {
					CurrentStatus = mAudioPlayer.GetChannelStatus( CurrentChannel );
				}
			}

			var track = GetTrack( channel );
			if( track != null ) {
				if( status == ePlaybackStatus.Stopped ) {
					track.PercentPlayed = mAudioPlayer.GetPercentPlayed( channel );
					mNoiseManager.PlayHistory.TrackPlayCompleted( track );
					mAudioPlayer.CloseChannel( channel );
					mOpenTracks.Remove( channel );

					if( channel == CurrentChannel ) {
						CurrentChannel = 0;

						StopInfoUpdate();

						if( mContinuePlaying ) {
							PlayNextTrack();
						}
					}
				}
			}
		}

		public void OnDatabaseItemChanged( DbItemChangedArgs args ) {
			var item = args.GetItem( mNoiseManager.DataProvider );

			if( item is DbTrack ) {
				var track = item as DbTrack;

				foreach( var queueItem in mOpenTracks.Values ) {
					if(( !queueItem.IsStream ) &&
					   ( queueItem.Track.DbId == track.DbId )) {
						queueItem.UpdateTrack( track );

						FirePlaybackTrackUpdate();
						break;
					}
				}
			}
		}

		public void OnStreamInfo( StreamInfo info ) {
			var track = GetTrack( info.Channel );

			if( track != null ) {
				track.StreamInfo = info;

				FirePlaybackTrackUpdate();
			}
		}

		public void Play() {
			if( CurrentStatus == ePlaybackStatus.Paused ) {
				mAudioPlayer.Play( CurrentChannel );
			}
			else {
				if(( mNoiseManager.PlayQueue.PlayingTrack == null ) &&
				   ( mNoiseManager.PlayQueue.NextTrack == null )) {
					mNoiseManager.PlayQueue.ReplayQueue();
				}

				StartTrack( mNoiseManager.PlayQueue.PlayingTrack ?? mNoiseManager.PlayQueue.PlayNextTrack());
			}
		}

		public bool CanPlay {
			get {
				var retValue = true;

				if( mNoiseManager != null ) {
					retValue = (!mNoiseManager.PlayQueue.IsQueueEmpty ) && ( CurrentStatus != ePlaybackStatus.Playing );
				}

				return( retValue );
			}
		}

		public void PlayNextTrack() {
			StartTrack( mNoiseManager.PlayQueue.PlayNextTrack());
		}

		public bool CanPlayNextTrack {
			get { return( mNoiseManager != null ? mNoiseManager.PlayQueue.NextTrack != null : false ); }
		}

		public void PlayPreviousTrack() {
			if(( CurrentTrack != null ) &&
			   ( mCurrentStatus != ePlaybackStatus.Stopped ) &&
			   ( mCurrentPosition > new TimeSpan( 0, 0, 5 ))) {
				PlayPosition = 0;
			}
			else {
				StartTrack( mNoiseManager.PlayQueue.PlayPreviousTrack());
			}
		}

		public bool CanPlayPreviousTrack {
			get {
				var retValue = false;

				if( mNoiseManager != null ) {
					if(( mNoiseManager.PlayQueue.PreviousTrack != null ) ||
					  (( CurrentTrack != null ) &&
					   ( mCurrentStatus != ePlaybackStatus.Stopped ) &&
					   ( mCurrentPosition > new TimeSpan( 0,0,5 )))) {
						retValue = true;
					}
				}

				return( retValue );
			}
		}

		private void StartTrack( PlayQueueTrack track ) {
			if( CurrentTrack != null ) {
				mAudioPlayer.FadeAndStop( CurrentChannel );
			}

			if( track != null ) {
				var	channel = track.IsStream ? mAudioPlayer.OpenStream( track.Stream ) : mAudioPlayer.OpenFile( track.File );

				mOpenTracks.Add( channel, track );
				CurrentChannel = channel;
				mAudioPlayer.Play( CurrentChannel );

				mCurrentLength = mAudioPlayer.GetLength( CurrentChannel );

				StartInfoUpdate();
				mContinuePlaying = true;

				FirePlaybackTrackStarted( track );
			}

			FirePlaybackTrackUpdate();
		}

		public void Pause() {
			if( CurrentTrack != null ) {
				mAudioPlayer.FadeAndPause( CurrentChannel );
			}
		}

		public bool CanPause {
			get{ return(( CurrentTrack != null ) && ( CurrentStatus == ePlaybackStatus.Playing )); }
		}

		public void Stop() {
			mContinuePlaying = false;

			foreach( int channel in mOpenTracks.Keys ) {
				mAudioPlayer.FadeAndStop( channel );
			}
		}

		public bool CanStop {
			get{ return( CurrentStatus == ePlaybackStatus.Paused || CurrentStatus == ePlaybackStatus.Playing ); }
		}

		private void OnInfoUpdateTimer( object sender, ElapsedEventArgs arg ) {
			if( CurrentChannel != 0 ) {
				mSampleLevels = mAudioPlayer.GetSampleLevels( CurrentChannel );
				mCurrentPosition = mAudioPlayer.GetPlayPosition( CurrentChannel );
			}
			else {
				mSampleLevels = new AudioLevels( 0.0, 0.0 );
			}

			FireInfoUpdate();
		}

		private void StartInfoUpdate() {
			mInfoUpdateTimer.Start();
		}

		private void StopInfoUpdate() {
			mInfoUpdateTimer.Stop();

			mSampleLevels = new AudioLevels( 0.0, 0.0 );
			mCurrentPosition = new TimeSpan();

			FireInfoUpdate();
		}

		public void ToggleTimeDisplay() {
			var	systemConfig = mContainer.Resolve<ISystemConfiguration>();
			var configuration = systemConfig.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

			if( configuration != null ) {
				configuration.DisplayPlayTimeElapsed = !configuration.DisplayPlayTimeElapsed;
				mDisplayTimeElapsed = configuration.DisplayPlayTimeElapsed;

				systemConfig.Save( configuration );
			}

			FireInfoUpdate();
		}

		public TimeSpan TrackTime {
			get {
				var retValue = new TimeSpan();

				if( CurrentChannel != 0 ) {
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
			get { return( CurrentChannel != 0 ? 
								( mCurrentPosition.Ticks < mCurrentLength.Ticks ? mCurrentPosition.Ticks : mCurrentLength.Ticks ) : 0L ); }
			set {
				if( CurrentChannel != 0 ) {
					mAudioPlayer.SetPlayPosition( CurrentChannel, new TimeSpan( value ));
				}
			}
		}

		public long TrackEndPosition {
			get { return( CurrentChannel != 0 ? mCurrentLength.Ticks : 1L ); }
		}

		public double Volume {
			get{ return( mAudioPlayer.Volume ); }
			set{
				mAudioPlayer.Volume = (float)value; 

				FireInfoUpdate();
			}
		}

		public double PlaySpeed {
			get{ return( mAudioPlayer.PlaySpeed ); }
			set {
				mAudioPlayer.PlaySpeed = (float)value;

				FireInfoUpdate();
			}
		}

		public double PanPosition {
			get{ return( mAudioPlayer.Pan ); }
			set {
				mAudioPlayer.Pan = (float)value;

				FireInfoUpdate();
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

		public bool IsFavorite {
			get {
				var	retValue = false;
				var settings = GetChannelSettings( CurrentChannel );

				if( settings != null ) {
					retValue = settings.IsFavorite;
				}

				return( retValue );
			}
			set {
				var settings = GetChannelSettings( CurrentChannel );
				if( settings != null ) {
					var track = GetTrack( CurrentChannel );

					settings.IsFavorite = value;

					if( track != null ) {
						GlobalCommands.SetFavorite.Execute( new SetFavoriteCommandArgs( track.Track.DbId, value ));
					}
					else {
						mNoiseManager.DataProvider.UpdateItem( settings );
					}
				}
			}
		}

		public Int16 Rating {
			get {
				Int16	retValue = 0;
				var		settings = GetChannelSettings( CurrentChannel );

				if( settings != null ) {
					retValue = settings.Rating;
				}

				return( retValue );
			}
			set {
				var settings = GetChannelSettings( CurrentChannel );

				if( settings != null ) {
					var track = GetTrack( CurrentChannel );

					settings.Rating = value;

					if( track.Track != null ) {
						GlobalCommands.SetRating.Execute( new SetRatingCommandArgs( track.Track.DbId, value ));
					}
					else {
						mNoiseManager.DataProvider.UpdateItem( settings );
					}
				}
			}
		}

		public double LeftLevel {
			get { return( CurrentChannel != 0 ? mSampleLevels.LeftLevel : 0.0 ); }
		}

		public double RightLevel {
			get { return( CurrentChannel != 0 ? mSampleLevels.RightLevel : 0.0 ); }
		}

		private void FireInfoUpdate() {
			mEvents.GetEvent<Events.PlaybackInfoChanged>().Publish( null );
		}

		private void FirePlaybackTrackUpdate() {
			mEvents.GetEvent<Events.PlaybackTrackChanged>().Publish( null );
		}

		private void FirePlaybackTrackStarted( PlayQueueTrack track ) {
			mEvents.GetEvent<Events.PlaybackTrackStarted>().Publish( track );
		}
	}
}
