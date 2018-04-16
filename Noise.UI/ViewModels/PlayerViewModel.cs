using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media;
using Caliburn.Micro;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Regions;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Dto;
using Noise.UI.Resources;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	[SyncActiveState]
	public class PlayerViewModel : ViewModelBase, IActiveAware,
								   IHandle<Events.SystemShutdown>,
								   IHandle<Events.PlaybackStatusChanged>, IHandle<Events.PlaybackTrackChanged>, IHandle<Events.PlaybackInfoChanged>,
								   IHandle<Events.PlaybackTrackStarted>, IHandle<Events.SongLyricsInfo>, IHandle<Events.PlaybackTrackUpdated>,
								   IHandle<Events.AudioParametersChanged> {
		private readonly IEventAggregator	mEventAggregator;
		private readonly IPlayQueue			mPlayQueue;
		private readonly IPlayController	mPlayController;
		private readonly IAudioController	mAudioController;
		private readonly IDialogService		mDialogService;
		private readonly IDisposable		mPlayStateChangeDisposable;
		private double						mSpectrumImageWidth;
		private double						mSpectrumImageHeight;
		private LyricsInfo					mLyricsInfo;
		private readonly Color				mBaseColor;
		private readonly Color				mPeakColor;
		private readonly Color				mPeakHoldColor;
		private readonly Timer				mSpectrumUpdateTimer;
		private ImageSource					mSpectrumBitmap;
		private	readonly ObservableCollectionEx<UiEqBand>	mBands;
		private readonly PlaybackContextDialogManager		mPlaybackContextDialogManager;

		public bool						IsActive { get; set; }
		public event EventHandler		IsActiveChanged = delegate { };

		public PlayerViewModel( IEventAggregator eventAggregator, IPlayQueue playQueue, IPlayController playController, IAudioController audioController,
								IDialogService dialogService, PlaybackContextDialogManager playbackContextDialogManager ) {
			mEventAggregator = eventAggregator;
			mPlayQueue = playQueue;
			mPlayController = playController;
			mAudioController = audioController;
			mDialogService = dialogService;
			mPlaybackContextDialogManager = playbackContextDialogManager;

			mSpectrumImageWidth = 200;
			mSpectrumImageHeight = 100;

			mBaseColor = ColorResources.SpectrumAnalyzerBaseColor;
			mPeakColor = ColorResources.SpectrumAnalyzerPeakColor;
			mPeakHoldColor = ColorResources.SpectrumAnalyzerPeakColor;

			mSpectrumUpdateTimer = new Timer { Enabled = false, Interval = 100 };
			mSpectrumUpdateTimer.Tick += OnSpectrumUpdateTimer;

			mBands = new ObservableCollectionEx<UiEqBand>();

			mEventAggregator.Subscribe( this );

			LoadBands();

			PlayState = ePlayState.StoppedEmptyQueue.ToString();
			mPlayStateChangeDisposable = mPlayController.PlayStateChange.Subscribe( OnPlayStateChange );

			IsActive = true; // default to the active state.
		}

		private void OnPlayStateChange( ePlayState state ) {
			PlayState = state.ToString();

			RaisePropertyChanged( () => PlayState );
		}

		public string PlayState {
			get {
#if DEBUG
				return( Get( () => PlayState ));
#else
				return( string.Empty );
#endif
			} 
			private set { Set( () => PlayState, value ); }
		}

		public void Handle( Events.SystemShutdown args ) {
			mSpectrumUpdateTimer.Stop();
			mEventAggregator.Unsubscribe( this );
			mPlayStateChangeDisposable.Dispose();
		}

		public void Handle( Events.PlaybackStatusChanged eventArgs ) {
			if( CurrentStatus != eventArgs.Status ) {
				CurrentStatus = eventArgs.Status;
			}
			else {
				RaisePropertyChanged( () => CurrentStatus );
			}
		}

		public void Handle( Events.PlaybackTrackChanged eventArgs ) {
			StartTrackFlag++;
		}

		public void Handle( Events.PlaybackInfoChanged eventArgs ) {
			InfoUpdateFlag++;
		}

		public void Handle( Events.PlaybackTrackUpdated eventArgs ) {
			// Update favorite and ratings if it's the playing track.
			if(( mPlayQueue.PlayingTrack != null ) &&
			   ( mPlayQueue.PlayingTrack.Name.Equals( eventArgs.Track.Name ))) {
				StartTrackFlag++;
			}
		}

		public void Handle( Events.AudioParametersChanged eventArgs ) {
			AudioParametersFlag++;
		}

		private ePlaybackStatus CurrentStatus {
			get { return( Get(() => CurrentStatus, ePlaybackStatus.Stopped ));  }
			set { Set(() => CurrentStatus, value ); }
		}

		private int StartTrackFlag {
			get{ return( Get( () => StartTrackFlag, 0 )); }
			set{ Set( () => StartTrackFlag, value  ); }
		}

		private int InfoUpdateFlag {
			get{ return( Get( () => InfoUpdateFlag, 0 ));  }
			set{ Execute.OnUIThread( () => Set( () => InfoUpdateFlag, value )); }
		}

		private int AudioParametersFlag {
			get { return(Get( () => AudioParametersFlag, 0 )); }
			set { Execute.OnUIThread( () => Set( () => AudioParametersFlag, value ));}
		}

		[DependsUpon( "StartTrackFlag" )]
		public string TrackName {
			get { 
				var retValue = string.Empty;

				if( mPlayQueue.PlayingTrack != null ) {
					var track = mPlayQueue.PlayingTrack;

					retValue = track.IsStream ? track.StreamInfo != null ? track.StreamInfo.Title : track.Stream.Name : track.Track.Name;
				}
				else if( IsInDesignMode ) {
					retValue = "The Flying Dutchmens Tribute";
				}

				return( retValue );
			} 
		}

		[DependsUpon( "StartTrackFlag")]
		public string ArtistName {
			get {
				var retValue = string.Empty;

				if( mPlayQueue.PlayingTrack != null ) {
					retValue = mPlayQueue.PlayingTrack.IsStream ? mPlayQueue.PlayingTrack.StreamInfo.Artist : mPlayQueue.PlayingTrack.Artist.Name;
				}

				return( retValue );
			}
		}

		[DependsUpon( "StartTrackFlag" )]
		public string AlbumName {
			get {
				var retValue = string.Empty;

				if( mPlayQueue.PlayingTrack != null ) {
					var track = mPlayQueue.PlayingTrack;

					retValue = track.IsStream ? track.StreamInfo != null ? track.StreamInfo.Album : track.Stream.Description :
												track.Album.Name;
				}
				else if( IsInDesignMode ) {
					retValue = "( The Trubedours/Timeless Hits and Classics)";
				}

				return( retValue );
			}
		}

		[DependsUpon( "StartTrackFlag" )]
		public string ArtistAlbumName {
			get {
				var retValue = string.Empty;

				if( mPlayQueue.PlayingTrack != null ) {
					var track = mPlayQueue.PlayingTrack;

					retValue = track.IsStream ? track.StreamInfo != null ? String.Format( " ({0}/{1})", track.StreamInfo.Artist, track.StreamInfo.Album ) :
													String.Format( " - {0}", track.Stream.Description ) :
												String.Format( " ({0}/{1})", track.Artist.Name, track.Album.Name );
				}
				else if( IsInDesignMode ) {
					retValue = "( The Trubedours/Timeless Hits and Classics)";
				}

				return( retValue );
			}
		}

		[DependsUpon( "StartTrackFlag" )]
		public PlayQueueTrack PeekNextTrack {
			get { return( mPlayController.NextTrack ); }
		}

		[DependsUpon( "StartTrackFlag" )]
		public PlayQueueTrack PeekPreviousTrack {
			get { return( mPlayController.PreviousTrack ); }
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public TimeSpan TrackTime {
			get { return( mPlayController.TrackTime ); }
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public double PlayPositionPercentage {
			get{ return( mPlayController.PlayPositionPercentage ); }
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public double PlayPositionPercentagePlus {
			get { return ( PlayPositionPercentage > 0.0D ? PlayPositionPercentage + 0.035D : 0.0D ); }
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public long TrackPosition {
			get { return( mPlayController.PlayPosition ); }
			set { mPlayController.PlayPosition = value; }
		}

		public void Execute_ToggleTimeDisplay() {
			mPlayController.ToggleTimeDisplay();
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public long TrackEndPosition {
			get { return( mPlayController.TrackEndPosition ); }
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public double Volume {
			get{ return( mAudioController.Volume ); }
			set{ mAudioController.Volume = (float)value; }
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public bool IsMuted {
			get{ return( mAudioController.Mute ); }
		}

		public void Execute_Mute() {
			mAudioController.Mute = !mAudioController.Mute;
		}

		[DependsUpon( "InfoUpdateFlag" )]
		[DependsUpon( "AudioParametersFlag" )]
		public double PlaySpeed {
			get{ return( mAudioController.PlaySpeed ); }
			set{ mAudioController.PlaySpeed = (float)value; }
		}

		public void Execute_ResetPlaySpeed() {
			mAudioController.SetDefaultPlaySpeed();
		}

		[DependsUpon( "InfoUpdateFlag" )]
		[DependsUpon( "AudioParametersFlag" )]
		public double PanPosition {
			get{ return( mAudioController.PanPosition ); }
			set{ mAudioController.PanPosition = (float)value; }
		}

		public void Execute_ResetPanPosition() {
			mAudioController.SetDefaultPanPosition();
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public double LeftLevel {
			get { return( mPlayController.LeftLevel ); }
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public double RightLevel {
			get { return( mPlayController.RightLevel ); }
		}

		[DependsUpon( "StartTrackFlag" )]
		[DependsUpon( "InfoUpdateFlag" )]
		public bool IsFavorite {
			get { return( mPlayController.IsFavorite ); }
			set { mPlayController.IsFavorite = value; }
		}

		[DependsUpon( "StartTrackFlag" )]
		[DependsUpon( "InfoUpdateFlag" )]
		public Int16 Rating {
			get{ return( mPlayController.Rating ); }
			set { mPlayController.Rating = value; }
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

		public void Execute_StopAtEndOfTrack( object sender ) {
			mPlayController.StopAtEndOfTrack();

			RaiseCanExecuteChangedEvent( "CanExecute_StopAtEndOfTrack" );
		}
		[DependsUpon( "CurrentStatus" )]
		public bool CanExecute_StopAtEndOfTrack( object sender ) {
			return ( mPlayController != null && mPlayController.CanStopAtEndOfTrack );
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

		[DependsUpon( "AudioParametersFlag" )]
		public bool TrackOverlapEnable {
			get{ return( mAudioController.TrackOverlapEnable ); }
			set{ mAudioController.TrackOverlapEnable = value; }
		}

		[DependsUpon( "AudioParametersFlag" )]
		public int TrackOverlapMilliseconds {
			get{ return( mAudioController.TrackOverlapMilliseconds ); }
			set{ mAudioController.TrackOverlapMilliseconds = value; }
		}

		public void Execute_PlayerSwitch() {
			mEventAggregator.Publish( new Events.ExternalPlayerSwitch());
		}

		private void OnSpectrumUpdateTimer( object sender, EventArgs args ) {
			if( IsActive ) {
				UpdateImage();

				RaisePropertyChanged( () => SpectrumImage );
			}
		}

		public ImageSource SpectrumImage {
			get {
				if(!mSpectrumUpdateTimer.Enabled ) {
					UpdateImage();
					mSpectrumUpdateTimer.Enabled = true;
				}

				return( mSpectrumBitmap );
			}
		}

		private void UpdateImage() {
			mSpectrumBitmap = mPlayController.GetSpectrumImage((int)mSpectrumImageHeight, (int)mSpectrumImageWidth, mBaseColor, mPeakColor, mPeakHoldColor );
		}

		public double ImageHeight {
			get{ return( mSpectrumImageHeight ); }
			set {
				if((!double.IsNaN( value )) &&
				   ( value >  0 )) {
					mSpectrumImageHeight = value;
				}
			}
		}

		public double ImageWidth {
			get{ return( mSpectrumImageWidth ); }
			set{
				if((!double.IsNaN( value )) &&
				   ( value > 0 )) {
					mSpectrumImageWidth = value; 
				}
			}
		}

		public IEnumerable<AudioDevice>	AudioDevices {
			get{ return( mAudioController.AudioDevices ); }
		}

		public AudioDevice CurrentAudioDevice {
			get{ return( mAudioController.CurrentAudioDevice ); }
			set {
				mPlayController.Stop();
				mAudioController.CurrentAudioDevice = value; 
				
				RaisePropertyChanged( () => CurrentAudioDevice );
			}
		}


		[DependsUpon( "AudioParametersFlag" )]
		public double PreampVolume {
			get{ return( mAudioController.PreampVolume ); }
			set {
				if( Math.Abs( mAudioController.PreampVolume - value ) > 0.01D ) {
					mAudioController.PreampVolume = value;

					RaisePropertyChanged( () => PreampVolume );
				}
			}
		}

		[DependsUpon( "AudioParametersFlag" )]
		public bool ReplayGainEnabled {
			get{ return( mPlayController.ReplayGainEnable ); }
			set{ mPlayController.ReplayGainEnable = value; }
		}

		public List<ParametricEqualizer> EqualizerList {
			get{ return( new List<ParametricEqualizer>( from ParametricEqualizer eq in mAudioController.EqManager.EqPresets orderby eq.Name ascending select eq )); }
		}

		public ParametricEqualizer CurrentEq {
			get{ return( mAudioController.CurrentEq ); }
			set {
				mAudioController.CurrentEq = value;
				Set( () => CurrentEq, value );

				LoadBands();
			}
		}

		[DependsUpon( "CurrentEq" )]
		public bool EqEnabled {
			get{ return( mAudioController.EqEnabled ); }
			set { mAudioController.EqEnabled = value; }
		}

		[DependsUpon( "CurrentEq" )]
		public ObservableCollection<UiEqBand> EqualizerBands {
			get{ return( mBands ); }
		}

		private void LoadBands() {
			mBands.Clear();

			if( mAudioController.CurrentEq != null ) {
				foreach( var band in mAudioController.EqManager.CurrentEq.Bands ) {
					mBands.Add( new UiEqBand( band, AdjustEq, mAudioController.EqManager.CurrentEq.IsPreset ));
				}
			}
		}

		private void AdjustEq( UiEqBand band ) {
			mAudioController.SetEqValue( band.BandId, band.Gain );

			if(!CurrentEq.IsPreset ) {
				mAudioController.EqManager.SaveEq( CurrentEq, EqEnabled );
			}
		}

		public void Execute_ResetBands() {
			foreach( var band in mBands ) {
				band.Gain = 0.0f;

				AdjustEq( band );
			}

			PreampVolume = 1.0f;
		}

		[DependsUpon( "CurrentEq" )]
		public bool IsEqEditable {
			get{ return(!CurrentEq.IsPreset ); }
		}

		[DependsUpon( "CurrentEq" )]
		public bool CanExecute_ResetBands() {
			return( IsEqEditable );
		}

		[DependsUpon( "AudioParametersFlag" )]
		public bool StereoEnhancerEnable {
			get{ return( mAudioController.StereoEnhancerEnable ); }
			set {
				mAudioController.StereoEnhancerEnable = value;

				RaisePropertyChanged( () => StereoEnhancerEnable );
			}
		}

		[DependsUpon( "AudioParametersFlag" )]
		public double StereoEnhancerWidth {
			get{ return( mAudioController.StereoEnhancerWidth ); }
			set{ mAudioController.StereoEnhancerWidth = value; }
		}

		[DependsUpon( "AudioParametersFlag" )]
		public double StereoEnhancerWetDry {
			get{ return( mAudioController.StereoEnhancerWetDry ); }
			set{ mAudioController.StereoEnhancerWetDry = value; }
		}

		[DependsUpon( "AudioParametersFlag" )]
		public bool SoftSaturationEnable {
			get{ return( mAudioController.SoftSaturationEnable ); }
			set {
				mAudioController.SoftSaturationEnable = value;

				RaisePropertyChanged( () => SoftSaturationEnable );
			}
		}

		[DependsUpon( "AudioParametersFlag" )]
		public double SoftSaturationDepth {
			get{ return( mAudioController.SoftSaturationDepth ); }
			set{ mAudioController.SoftSaturationDepth = value; }
		}

		[DependsUpon( "AudioParametersFlag" )]
		public double SoftSaturationFactor {
			get{ return( mAudioController.SoftSaturationFactor ); }
			set{ mAudioController.SoftSaturationFactor = value; }
		}

		[DependsUpon( "AudioParametersFlag" )]
		public bool ReverbEnable {
			get{ return( mAudioController.ReverbEnable ); }
			set {
				mAudioController.ReverbEnable = value;

				RaisePropertyChanged( () => ReverbEnable );
			}
		}

		[DependsUpon( "AudioParametersFlag" )]
		public float ReverbLevel {
			get{ return( mAudioController.ReverbLevel ); }
			set{ mAudioController.ReverbLevel = value; }
		}

		[DependsUpon( "AudioParametersFlag" )]
		public float ReverbDelay {
			get{ return( mAudioController.ReverbDelay ); }
			set{ mAudioController.ReverbDelay = value; }
		}

		public void Execute_StandardPlayer() {
			mEventAggregator.Publish( new Events.StandardPlayerRequest());
		}

		public void Execute_ExtendedPlayer() {
			mEventAggregator.Publish( new Events.ExtendedPlayerRequest());
		}

		public void Execute_RequestSimilarSongSearch() {
			if(( mPlayController.CurrentTrack != null ) &&
			   ( mPlayController.CurrentTrack.Track != null )) {
				mEventAggregator.Publish( new Events.SimilarSongSearchRequest( mPlayController.CurrentTrack.Track.DbId ));
			}
		}

		[DependsUpon("StartTrackFlag")]
		public bool CanExecute_RequestSimilarSongSearch() {
			return(( mPlayController.CurrentTrack != null ) &&
			       ( mPlayController.CurrentTrack.Track != null ));
		}

		public void Handle( Events.PlaybackTrackStarted eventArgs ) {
			mLyricsInfo = null;
			RaiseCanExecuteChangedEvent( "CanExecute_RequestLyrics" );
		}

		public void Handle( Events.SongLyricsInfo eventArgs ) {
			mLyricsInfo = eventArgs.LyricsInfo;

			RaiseCanExecuteChangedEvent( "CanExecute_RequestLyrics" );
		}

		public void Execute_RequestLyrics() {
			if( mLyricsInfo != null ) {
				mEventAggregator.Publish( new Events.SongLyricsRequest( mLyricsInfo ));
			}
		}

		public bool CanExecute_RequestLyrics() {
			return( mLyricsInfo != null );
		}

		public void Execute_TrackInformation() {
			if( mPlayController.CurrentTrack.Album != null ) {
				mEventAggregator.Publish( new Events.AlbumFocusRequested( mPlayController.CurrentTrack.Album ));
			}
		}

		[DependsUpon("StartTrackFlag")]
		public bool CanExecute_TrackInformation() {
			return(( mPlayController.CurrentTrack != null ) &&
			       ( mPlayController.CurrentTrack.Album != null ));
		}

		public void Execute_ManagePlaybackContext() {
			if(( mPlayController.CurrentTrack != null ) &&
			   ( mDialogService != null ) &&
			   ( mPlaybackContextDialogManager != null )) {
				mPlaybackContextDialogManager.SetTrack( mPlayController.CurrentTrack.Album, mPlayController.CurrentTrack.Track );

				if( mDialogService.ShowDialog( DialogNames.ManagePlaybackContext, mPlaybackContextDialogManager ) == true ) {
					mPlaybackContextDialogManager.UpdatePlaybackContext();
				}
			}
		}

		[DependsUpon("StartTrackFlag")]
		public bool CanExecute_ManagePlaybackContext() {
			return(( mDialogService != null ) && 
				   ( mPlaybackContextDialogManager != null ) &&
				   ( mPlayController.CurrentTrack != null ));
		}

	}
}
