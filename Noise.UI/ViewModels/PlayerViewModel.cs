using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media;
using Caliburn.Micro;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Events;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Dto;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class PlayerViewModel : ViewModelBase, IActiveAware,
								   IHandle<Events.PlaybackStatusChanged>, IHandle<Events.PlaybackTrackChanged>, IHandle<Events.PlaybackInfoChanged>,
								   IHandle<Events.PlaybackTrackStarted> {
		private readonly IEventAggregator	mEvents;
		private readonly ICaliburnEventAggregator	mEventAggregator;
		private readonly IPlayQueue			mPlayQueue;
		private readonly IPlayController	mPlayController;
		private double						mSpectrumImageWidth;
		private double						mSpectrumImageHeight;
		private LyricsInfo					mLyricsInfo;
		private readonly Color				mBaseColor;
		private readonly Color				mPeakColor;
		private readonly Color				mPeakHoldColor;
		private readonly Timer				mSpectrumUpdateTimer;
		private ImageSource					mSpectrumBitmap;
		private	readonly ObservableCollectionEx<UiEqBand>	mBands;

		public bool						IsActive { get; set; }
		public event EventHandler		IsActiveChanged = delegate { };

		public PlayerViewModel( IEventAggregator eventAggregator, ICaliburnEventAggregator caliburnEventAggregator, IPlayQueue playQueue, IPlayController playController ) {
			mEvents = eventAggregator;
			mEventAggregator = caliburnEventAggregator;
			mPlayQueue = playQueue;
			mPlayController = playController;

			mSpectrumImageWidth = 200;
			mSpectrumImageHeight = 100;

			mBaseColor = Colors.LightBlue;
			mPeakColor = Colors.Blue;
			mPeakHoldColor = Colors.Blue;

			mSpectrumUpdateTimer = new Timer { Enabled = false, Interval = 100 };
			mSpectrumUpdateTimer.Tick += OnSpectrumUpdateTimer;

			mBands = new ObservableCollectionEx<UiEqBand>();

			mEventAggregator.Subscribe( this );

			mEvents.GetEvent<Events.SongLyricsInfo>().Subscribe( OnSongLyricsInfo );

			LoadBands();

			PlayState = ePlayState.StoppedEmptyQueue.ToString();
			mPlayController.PlayStateChange.Subscribe( OnPlayStateChange );
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
				return( "" );
#endif
			} 
			private set { Set( () => PlayState, value ); }
		}


		public void Handle( Events.PlaybackStatusChanged eventArgs ) {
			CurrentStatus = eventArgs.Status;
		}

		public void Handle( Events.PlaybackTrackChanged eventArgs ) {
			StartTrackFlag++;
		}

		public void Handle( Events.PlaybackInfoChanged eventArgs ) {
			InfoUpdateFlag++;
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

		[DependsUpon( "StartTrackFlag" )]
		public string TrackName {
			get { 
				var retValue = string.Empty;

				if( mPlayQueue.PlayingTrack != null ) {
					var track = mPlayQueue.PlayingTrack;

					retValue = track.IsStream ? track.StreamInfo != null ? String.Format( "{0} ({1}/{2})", track.StreamInfo.Title, track.StreamInfo.Artist, track.StreamInfo.Album ) :
													String.Format( "{0} - {1}", track.Stream.Name, track.Stream.Description ) :
												String.Format( "{0} ({1}/{2})", track.Track.Name, track.Artist.Name, track.Album.Name );
				}
				else if( IsInDesignMode ) {
					retValue = "The Flying Dutchmens Tribute";
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
			get{ return( mPlayController.Volume ); }
			set{ mPlayController.Volume = (float)value; }
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public bool IsMuted {
			get{ return( mPlayController.Mute ); }
		}

		public void Execute_Mute() {
			mPlayController.Mute = !mPlayController.Mute;
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public double PlaySpeed {
			get{ return( mPlayController.PlaySpeed ); }
			set{ mPlayController.PlaySpeed = (float)value; }
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public double PanPosition {
			get{ return( mPlayController.PanPosition ); }
			set{ mPlayController.PanPosition = (float)value; }
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
		public bool IsFavorite {
			get { return( mPlayController.IsFavorite ); }
			set { mPlayController.IsFavorite = value; }
		}

		[DependsUpon( "StartTrackFlag" )]
		public Int16 Rating {
			get{ return( mPlayController.Rating ); }
			set { mPlayController.Rating = value; }
		}

		public void Execute_Play( object sender ) {
			mPlayController.Play();
		}
		[DependsUpon( "CurrentStatus" )]
		public bool CanExecute_Play( object sender ) {
			return( mPlayController.CanPlay );
		}

		public void Execute_Pause( object sender ) {
			mPlayController.Pause();
		}
		[DependsUpon( "CurrentStatus" )]
		public bool CanExecute_Pause( object sender ) {
			return( mPlayController.CanPause );
		}

		public void Execute_Stop( object sender ) {
			mPlayController.Stop();
		}
		[DependsUpon( "CurrentStatus" )]
		public bool CanExecute_Stop( object sender ) {
			return( mPlayController.CanStop );
		}

		public void Execute_NextTrack( object sender ) {
			mPlayController.PlayNextTrack();
		}
		[DependsUpon( "CurrentStatus" )]
		public bool CanExecute_NextTrack( object sender ) {
			return( mPlayController.CanPlayNextTrack );
		}

		public void Execute_PreviousTrack( object sender ) {
			mPlayController.PlayPreviousTrack();
		}
		[DependsUpon( "CurrentStatus" )]
		[DependsUpon( "InfoUpdateFlag" )]
		public bool CanExecute_PreviousTrack( object sender ) {
			return( mPlayController.CanPlayPreviousTrack );
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

		public bool TrackOverlapEnable {
			get{ return( mPlayController.TrackOverlapEnable ); }
			set{ mPlayController.TrackOverlapEnable = value; }
		}

		public int TrackOverlapMilliseconds {
			get{ return( mPlayController.TrackOverlapMilliseconds ); }
			set{ mPlayController.TrackOverlapMilliseconds = value; }
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

		public double PreampVolume {
			get{ return( mPlayController.PreampVolume ); }
			set{ mPlayController.PreampVolume = value; }
		}

		public bool ReplayGainEnabled {
			get{ return( mPlayController.ReplayGainEnable ); }
			set{ mPlayController.ReplayGainEnable = value; }
		}

		public List<ParametricEqualizer> EqualizerList {
			get{ return( new List<ParametricEqualizer>( from ParametricEqualizer eq in mPlayController.EqManager.EqPresets orderby eq.Name ascending select eq )); }
		}

		public ParametricEqualizer CurrentEq {
			get{ return( mPlayController.CurrentEq ); }
			set {
				mPlayController.CurrentEq = value;
				Set( () => CurrentEq, value );

				LoadBands();
			}
		}

		[DependsUpon( "CurrentEq" )]
		public bool EqEnabled {
			get{ return( mPlayController.EqEnabled ); }
			set {
				mPlayController.EqEnabled = value;
				mPlayController.EqManager.SaveEq( CurrentEq, value );
			}
		}

		[DependsUpon( "CurrentEq" )]
		public ObservableCollection<UiEqBand> EqualizerBands {
			get{ return( mBands ); }
		}

		private void LoadBands() {
			mBands.Clear();

			if( mPlayController.CurrentEq != null ) {
				foreach( var band in mPlayController.EqManager.CurrentEq.Bands ) {
					mBands.Add( new UiEqBand( band, AdjustEq, mPlayController.EqManager.CurrentEq.IsPreset ));
				}
			}
		}

		private void AdjustEq( UiEqBand band ) {
			mPlayController.SetEqValue( band.BandId, band.Gain );

			if(!CurrentEq.IsPreset ) {
				mPlayController.EqManager.SaveEq( CurrentEq, EqEnabled );
			}
		}

		public void Execute_ResetBands() {
			foreach( var band in mBands ) {
				band.Gain = 0.0f;

				AdjustEq( band );
			}

			mPlayController.PreampVolume = 1.0f;

			RaisePropertyChanged( () => PreampVolume );
		}

		[DependsUpon( "CurrentEq" )]
		public bool IsEqEditable {
			get{ return(!CurrentEq.IsPreset ); }
		}

		[DependsUpon( "CurrentEq" )]
		public bool CanExecute_ResetBands() {
			return( IsEqEditable );
		}

		public bool StereoEnhancerEnable {
			get{ return( mPlayController.StereoEnhancerEnable ); }
			set {
				mPlayController.StereoEnhancerEnable = value;

				RaisePropertyChanged( () => StereoEnhancerEnable );
			}
		}

		public double StereoEnhancerWidth {
			get{ return( mPlayController.StereoEnhancerWidth ); }
			set{ mPlayController.StereoEnhancerWidth = value; }
		}

		public double StereoEnhancerWetDry {
			get{ return( mPlayController.StereoEnhancerWetDry ); }
			set{ mPlayController.StereoEnhancerWetDry = value; }
		}

		public bool SoftSaturationEnable {
			get{ return( mPlayController.SoftSaturationEnable ); }
			set {
				mPlayController.SoftSaturationEnable = value;

				RaisePropertyChanged( () => SoftSaturationEnable );
			}
		}

		public double SoftSaturationDepth {
			get{ return( mPlayController.SoftSaturationDepth ); }
			set{ mPlayController.SoftSaturationDepth = value; }
		}

		public double SoftSaturationFactor {
			get{ return( mPlayController.SoftSaturationFactor ); }
			set{ mPlayController.SoftSaturationFactor = value; }
		}

		public bool ReverbEnable {
			get{ return( mPlayController.ReverbEnable ); }
			set {
				mPlayController.ReverbEnable = value;

				RaisePropertyChanged( () => ReverbEnable );
			}
		}

		public float ReverbLevel {
			get{ return( mPlayController.ReverbLevel ); }
			set{ mPlayController.ReverbLevel = value; }
		}

		public int ReverbDelay {
			get{ return( mPlayController.ReverbDelay ); }
			set{ mPlayController.ReverbDelay = value; }
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
				mEvents.GetEvent<Events.SimilarSongSearchRequest>().Publish( mPlayController.CurrentTrack.Track.DbId );
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

		private void OnSongLyricsInfo( LyricsInfo info ) {
			mLyricsInfo = info;

			RaiseCanExecuteChangedEvent( "CanExecute_RequestLyrics" );
		}

		public void Execute_RequestLyrics() {
			if( mLyricsInfo != null ) {
				mEvents.GetEvent<Events.SongLyricsRequest>().Publish( mLyricsInfo );
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
	}
}
