using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Dto;

namespace Noise.UI.ViewModels {
	public class PlayerViewModel : ViewModelBase, IActiveAware {
		private	IUnityContainer			mContainer;
		private IEventAggregator		mEvents;
		private INoiseManager			mNoiseManager;
		private double					mSpectrumImageWidth;
		private double					mSpectrumImageHeight;
		private LyricsInfo				mLyricsInfo;
		private readonly Color			mBaseColor;
		private readonly Color			mPeakColor;
		private readonly Color			mPeakHoldColor;
		private readonly Timer			mSpectrumUpdateTimer;
		private ImageSource				mSpectrumBitmap;
		private	readonly ObservableCollectionEx<UiEqBand>	mBands;

		public bool						IsActive { get; set; }
		public event EventHandler		IsActiveChanged = delegate { };

		public PlayerViewModel() {
			mSpectrumImageWidth = 200;
			mSpectrumImageHeight = 100;

			mBaseColor = Colors.LightBlue;
			mPeakColor = Colors.Blue;
			mPeakHoldColor = Colors.Blue;

			mSpectrumUpdateTimer = new Timer { Enabled = false, Interval = 100 };
			mSpectrumUpdateTimer.Tick += OnSpectrumUpdateTimer;

			mBands = new ObservableCollectionEx<UiEqBand>();
		}

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;

				mEvents = mContainer.Resolve<IEventAggregator>();
				mNoiseManager = mContainer.Resolve<INoiseManager>();

				mEvents.GetEvent<Events.PlaybackStatusChanged>().Subscribe( OnPlaybackStatusChanged );
				mEvents.GetEvent<Events.PlaybackTrackChanged>().Subscribe( OnPlaybackTrackChanged );
				mEvents.GetEvent<Events.PlaybackInfoChanged>().Subscribe( OnPlaybackInfoChanged );
				mEvents.GetEvent<Events.SongLyricsInfo>().Subscribe( OnSongLyricsInfo );
				mEvents.GetEvent<Events.PlaybackTrackStarted>().Subscribe( OnPlaybackTrackStarted );

				LoadBands();

				PlayState = ePlayState.StoppedEmptyQueue.ToString();
				mNoiseManager.PlayController.PlayStateChange.Subscribe( OnPlayStateChange );
			}
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


		public void OnPlaybackStatusChanged( ePlaybackStatus status ) {
			CurrentStatus = status;
		}

		public void OnPlaybackTrackChanged( object sender ) {
			StartTrackFlag++;
		}

		public void OnPlaybackInfoChanged( object sender ) {
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
			set{ Invoke( () => Set( () => InfoUpdateFlag, value )); }
		}

		[DependsUpon( "StartTrackFlag" )]
		public string TrackName {
			get { 
				var retValue = string.Empty;

				if( mNoiseManager.PlayQueue.PlayingTrack != null ) {
					var track = mNoiseManager.PlayQueue.PlayingTrack;

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
			get { return( mNoiseManager.PlayController.NextTrack ); }
		}

		[DependsUpon( "StartTrackFlag" )]
		public PlayQueueTrack PeekPreviousTrack {
			get { return( mNoiseManager.PlayController.PreviousTrack ); }
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public TimeSpan TrackTime {
			get { return( mNoiseManager.PlayController.TrackTime ); }
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public long TrackPosition {
			get { return( mNoiseManager.PlayController.PlayPosition ); }
			set { mNoiseManager.PlayController.PlayPosition = value; }
		}

		public void Execute_ToggleTimeDisplay() {
			mNoiseManager.PlayController.ToggleTimeDisplay();
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public long TrackEndPosition {
			get { return( mNoiseManager.PlayController.TrackEndPosition ); }
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public double Volume {
			get{ return( mNoiseManager.PlayController.Volume ); }
			set{ mNoiseManager.PlayController.Volume = (float)value; }
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public bool IsMuted {
			get{ return( mNoiseManager.PlayController.Mute ); }
		}

		public void Execute_Mute() {
			mNoiseManager.PlayController.Mute = !mNoiseManager.PlayController.Mute;
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public double PlaySpeed {
			get{ return( mNoiseManager.PlayController.PlaySpeed ); }
			set{ mNoiseManager.PlayController.PlaySpeed = (float)value; }
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public double PanPosition {
			get{ return( mNoiseManager.PlayController.PanPosition ); }
			set{ mNoiseManager.PlayController.PanPosition = (float)value; }
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public double LeftLevel {
			get { return( mNoiseManager.PlayController.LeftLevel ); }
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public double RightLevel {
			get { return( mNoiseManager.PlayController.RightLevel ); }
		}

		[DependsUpon( "StartTrackFlag" )]
		public bool IsFavorite {
			get { return( mNoiseManager.PlayController.IsFavorite ); }
			set { mNoiseManager.PlayController.IsFavorite = value; }
		}

		[DependsUpon( "StartTrackFlag" )]
		public Int16 Rating {
			get{ return( mNoiseManager.PlayController.Rating ); }
			set { mNoiseManager.PlayController.Rating = value; }
		}

		public void Execute_Play( object sender ) {
			mNoiseManager.PlayController.Play();
		}
		[DependsUpon( "CurrentStatus" )]
		public bool CanExecute_Play( object sender ) {
			return( mNoiseManager.PlayController.CanPlay );
		}

		public void Execute_Pause( object sender ) {
			mNoiseManager.PlayController.Pause();
		}
		[DependsUpon( "CurrentStatus" )]
		public bool CanExecute_Pause( object sender ) {
			return( mNoiseManager.PlayController.CanPause );
		}

		public void Execute_Stop( object sender ) {
			mNoiseManager.PlayController.Stop();
		}
		[DependsUpon( "CurrentStatus" )]
		public bool CanExecute_Stop( object sender ) {
			return( mNoiseManager.PlayController.CanStop );
		}

		public void Execute_NextTrack( object sender ) {
			mNoiseManager.PlayController.PlayNextTrack();
		}
		[DependsUpon( "CurrentStatus" )]
		public bool CanExecute_NextTrack( object sender ) {
			return( mNoiseManager.PlayController.CanPlayNextTrack );
		}

		public void Execute_PreviousTrack( object sender ) {
			mNoiseManager.PlayController.PlayPreviousTrack();
		}
		[DependsUpon( "CurrentStatus" )]
		[DependsUpon( "InfoUpdateFlag" )]
		public bool CanExecute_PreviousTrack( object sender ) {
			return( mNoiseManager.PlayController.CanPlayPreviousTrack );
		}

		public void Execute_ReplayTrack() {
			if( mNoiseManager != null ) {
				mNoiseManager.PlayQueue.PlayingTrackReplayCount++;
			}
			RaiseCanExecuteChangedEvent( "CanExecute_ReplayTrack" );
		}
		[DependsUpon( "CurrentStatus" )]
		[DependsUpon( "StartTrackFlag" )]
		public bool CanExecute_ReplayTrack() {
			var retValue = false;

			if(( mNoiseManager != null ) &&
			   ( mNoiseManager.PlayController.CurrentTrack != null ) &&
			   ( mNoiseManager.PlayQueue.PlayingTrackReplayCount == 0 )) {
				retValue = true;
			}
			return( retValue );
		}

		public bool TrackOverlapEnable {
			get{ return( mNoiseManager.PlayController.TrackOverlapEnable ); }
			set{ mNoiseManager.PlayController.TrackOverlapEnable = value; }
		}

		public int TrackOverlapMilliseconds {
			get{ return( mNoiseManager.PlayController.TrackOverlapMilliseconds ); }
			set{ mNoiseManager.PlayController.TrackOverlapMilliseconds = value; }
		}

		public void Execute_PlayerSwitch() {
			mEvents.GetEvent<Events.ExternalPlayerSwitch>().Publish( this );
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
			mSpectrumBitmap = mNoiseManager.PlayController.GetSpectrumImage((int)mSpectrumImageHeight, (int)mSpectrumImageWidth, mBaseColor, mPeakColor, mPeakHoldColor );
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
			get{ return( mNoiseManager.PlayController.PreampVolume ); }
			set{ mNoiseManager.PlayController.PreampVolume = value; }
		}

		public bool ReplayGainEnabled {
			get{ return( mNoiseManager.PlayController.ReplayGainEnable ); }
			set{ mNoiseManager.PlayController.ReplayGainEnable = value; }
		}

		public List<ParametricEqualizer> EqualizerList {
			get{ return( new List<ParametricEqualizer>( from ParametricEqualizer eq in mNoiseManager.PlayController.EqManager.EqPresets orderby eq.Name ascending select eq )); }
		}

		public ParametricEqualizer CurrentEq {
			get{ return( mNoiseManager.PlayController.CurrentEq ); }
			set {
				mNoiseManager.PlayController.CurrentEq = value;
				Set( () => CurrentEq, value );

				LoadBands();
			}
		}

		[DependsUpon( "CurrentEq" )]
		public bool EqEnabled {
			get{ return( mNoiseManager.PlayController.EqEnabled ); }
			set {
				mNoiseManager.PlayController.EqEnabled = value;
				mNoiseManager.PlayController.EqManager.SaveEq( CurrentEq, value );
			}
		}

		[DependsUpon( "CurrentEq" )]
		public ObservableCollection<UiEqBand> EqualizerBands {
			get{ return( mBands ); }
		}

		private void LoadBands() {
			mBands.Clear();

			if( mNoiseManager.PlayController.CurrentEq != null ) {
				foreach( var band in mNoiseManager.PlayController.EqManager.CurrentEq.Bands ) {
					mBands.Add( new UiEqBand( band, AdjustEq, mNoiseManager.PlayController.EqManager.CurrentEq.IsPreset ));
				}
			}
		}

		private void AdjustEq( UiEqBand band ) {
			mNoiseManager.PlayController.SetEqValue( band.BandId, band.Gain );

			if(!CurrentEq.IsPreset ) {
				mNoiseManager.PlayController.EqManager.SaveEq( CurrentEq, EqEnabled );
			}
		}

		public void Execute_ResetBands() {
			foreach( var band in mBands ) {
				band.Gain = 0.0f;

				AdjustEq( band );
			}

			mNoiseManager.PlayController.PreampVolume = 1.0f;

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
			get{ return( mNoiseManager.PlayController.StereoEnhancerEnable ); }
			set {
				mNoiseManager.PlayController.StereoEnhancerEnable = value;

				RaisePropertyChanged( () => StereoEnhancerEnable );
			}
		}

		public double StereoEnhancerWidth {
			get{ return( mNoiseManager.PlayController.StereoEnhancerWidth ); }
			set{ mNoiseManager.PlayController.StereoEnhancerWidth = value; }
		}

		public double StereoEnhancerWetDry {
			get{ return( mNoiseManager.PlayController.StereoEnhancerWetDry ); }
			set{ mNoiseManager.PlayController.StereoEnhancerWetDry = value; }
		}

		public bool SoftSaturationEnable {
			get{ return( mNoiseManager.PlayController.SoftSaturationEnable ); }
			set {
				mNoiseManager.PlayController.SoftSaturationEnable = value;

				RaisePropertyChanged( () => SoftSaturationEnable );
			}
		}

		public double SoftSaturationDepth {
			get{ return( mNoiseManager.PlayController.SoftSaturationDepth ); }
			set{ mNoiseManager.PlayController.SoftSaturationDepth = value; }
		}

		public double SoftSaturationFactor {
			get{ return( mNoiseManager.PlayController.SoftSaturationFactor ); }
			set{ mNoiseManager.PlayController.SoftSaturationFactor = value; }
		}

		public bool ReverbEnable {
			get{ return( mNoiseManager.PlayController.ReverbEnable ); }
			set {
				mNoiseManager.PlayController.ReverbEnable = value;

				RaisePropertyChanged( () => ReverbEnable );
			}
		}

		public float ReverbLevel {
			get{ return( mNoiseManager.PlayController.ReverbLevel ); }
			set{ mNoiseManager.PlayController.ReverbLevel = value; }
		}

		public int ReverbDelay {
			get{ return( mNoiseManager.PlayController.ReverbDelay ); }
			set{ mNoiseManager.PlayController.ReverbDelay = value; }
		}

		public void Execute_StandardPlayer() {
			mEvents.GetEvent<Events.StandardPlayerRequest>().Publish( this );
		}

		public void Execute_ExtendedPlayer() {
			mEvents.GetEvent<Events.ExtendedPlayerRequest>().Publish( this );
		}

		public void Execute_RequestSimilarSongSearch() {
			if(( mNoiseManager.PlayController.CurrentTrack != null ) &&
			   ( mNoiseManager.PlayController.CurrentTrack.Track != null )) {
				mEvents.GetEvent<Events.SimilarSongSearchRequest>().Publish( mNoiseManager.PlayController.CurrentTrack.Track.DbId );
			}
		}

		[DependsUpon("StartTrackFlag")]
		public bool CanExecute_RequestSimilarSongSearch() {
			var retValue = false;

			if(( mNoiseManager.PlayController.CurrentTrack != null ) &&
			   ( mNoiseManager.PlayController.CurrentTrack.Track != null )) {
				retValue = true;
			}

			return( retValue );
		}

		private void OnPlaybackTrackStarted( PlayQueueTrack track ) {
			mLyricsInfo = null;
			RaiseCanExecuteChangedEvent( "CanExecute_RequestLyrics" );

			GlobalCommands.RequestLyrics.Execute( new LyricsRequestArgs( track.Artist, track.Track ));
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
	}
}
