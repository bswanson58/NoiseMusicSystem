using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Dto;

namespace Noise.UI.ViewModels {
	public class PlayerViewModel : ViewModelBase {
		private	IUnityContainer			mContainer;
		private IEventAggregator		mEvents;
		private INoiseManager			mNoiseManager;
		private double					mSpectrumImageWidth;
		private double					mSpectrumImageHeight;
		private readonly Color			mBaseColor;
		private readonly Color			mPeakColor;
		private readonly Color			mPeakHoldColor;
		private readonly Timer			mSpectrumUpdateTimer;
		private ImageSource				mSpectrumBitmap;
		private	readonly ObservableCollectionEx<UiEqBand>	mBands;

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

				mEvents.GetEvent<Events.PlayQueueChanged>().Subscribe( OnPlayQueueChanged );
				mEvents.GetEvent<Events.PlaybackStatusChanged>().Subscribe( OnPlaybackStatusChanged );
				mEvents.GetEvent<Events.PlaybackTrackChanged>().Subscribe( OnPlaybackTrackChanged );
				mEvents.GetEvent<Events.PlaybackInfoChanged>().Subscribe( OnPlaybackInfoChanged );

				LoadBands();
			}
		}

		public void OnPlayQueueChanged( IPlayQueue playQueue ) {
			PlayQueueChangedFlag++;
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

		private int PlayQueueChangedFlag {
			get{ return( Get( () => PlayQueueChangedFlag, 0 )); }
			set{ Set( () => PlayQueueChangedFlag, value  ); }
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
		[DependsUpon( "PlayQueueChangedFlag" )]
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
		[DependsUpon( "PlayQueueChangedFlag" )]
		public bool IsFavorite {
			get { return( mNoiseManager.PlayController.IsFavorite ); }
			set { mNoiseManager.PlayController.IsFavorite = value; }
		}

		[DependsUpon( "StartTrackFlag" )]
		[DependsUpon( "PlayQueueChangedFlag" )]
		public Int16 Rating {
			get{ return( mNoiseManager.PlayController.Rating ); }
			set { mNoiseManager.PlayController.Rating = value; }
		}

		public void Execute_Play( object sender ) {
			mNoiseManager.PlayController.Play();
		}
		[DependsUpon( "CurrentStatus" )]
		[DependsUpon( "PlayQueueChangedFlag" )]
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
		[DependsUpon( "PlayQueueChangedFlag" )]
		[DependsUpon( "StartTrackFlag" )]
		public bool CanExecute_NextTrack( object sender ) {
			return( mNoiseManager.PlayController.CanPlayNextTrack );
		}

		public void Execute_PreviousTrack( object sender ) {
			mNoiseManager.PlayController.PlayPreviousTrack();
		}
		[DependsUpon( "PlayQueueChangedFlag" )]
		[DependsUpon( "StartTrackFlag" )]
		[DependsUpon( "InfoUpdateFlag" )]
		public bool CanExecute_PreviousTrack( object sender ) {
			return( mNoiseManager.PlayController.CanPlayPreviousTrack );
		}

		public void Execute_ClearQueue( object sender ) {
			if( mNoiseManager != null ) {
				mNoiseManager.PlayQueue.ClearQueue();
			}
		}
		[DependsUpon( "PlayQueueChangedFlag" )]
		public bool CanExecute_ClearQueue( object sender ) {
			var retValue = true;

			if( mNoiseManager != null ) {
				retValue = !mNoiseManager.PlayQueue.IsQueueEmpty;
			}

			return( retValue );
		}

		public void Execute_ReplayTrack() {
			if( mNoiseManager != null ) {
				mNoiseManager.PlayQueue.PlayingTrackReplayCount++;
			}
			RaiseCanExecuteChangedEvent( "CanExecute_ReplayTrack" );
		}
		[DependsUpon( "CurrentStatus" )]
		[DependsUpon( "PlayQueueChangedFlag" )]
		[DependsUpon( "StartTrackFlag" )]
		public bool CanExecute_ReplayTrack() {
			var retValue = false;

			if(( mNoiseManager != null ) &&
			   ( mNoiseManager.PlayController.CanStop ) &&
			   ( mNoiseManager.PlayQueue.PlayingTrackReplayCount == 0 )) {
				retValue = true;
			}
			return( retValue );
		}

		public void Execute_PlayerSwitch() {
			mEvents.GetEvent<Events.ExternalPlayerSwitch>().Publish( this );
		}

		private void OnSpectrumUpdateTimer( object sender, EventArgs args ) {
			UpdateImage();

			RaisePropertyChanged( () => SpectrumImage );
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
	}
}
