using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Dto;
using Noise.UI.Resources;
using Noise.UI.Views;
using Prism;
using Prism.Regions;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	[SyncActiveState]
	public class PlayerViewModel : AutomaticCommandBase, IActiveAware,
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
        private	readonly ObservableCollectionEx<UiEqBand>	mBands;
        private readonly Color				mBaseColor;
        private readonly Color				mPeakColor;
        private readonly Color				mPeakHoldColor;
        private readonly Timer				mSpectrumUpdateTimer;
		private double						mSpectrumImageWidth;
		private double						mSpectrumImageHeight;
		private LyricsInfo					mLyricsInfo;
		private ImageSource					mSpectrumBitmap;
		private	bool						mIsActive;

		public event EventHandler		IsActiveChanged = delegate { };

		public PlayerViewModel( IEventAggregator eventAggregator, IPlayQueue playQueue, IPlayController playController, IAudioController audioController, IDialogService dialogService ) {
			mEventAggregator = eventAggregator;
			mPlayQueue = playQueue;
			mPlayController = playController;
			mAudioController = audioController;
			mDialogService = dialogService;

			mSpectrumImageWidth = 200;
			mSpectrumImageHeight = 100;

			mBaseColor = ColorResources.SpectrumAnalyzerBaseColor;
			mPeakColor = ColorResources.SpectrumAnalyzerPeakColor;
			mPeakHoldColor = ColorResources.SpectrumAnalyzerHoldColor;

			mSpectrumUpdateTimer = new Timer { Enabled = false, Interval = 100 };
			mSpectrumUpdateTimer.Tick += OnSpectrumUpdateTimer;

			mBands = new ObservableCollectionEx<UiEqBand>();

			mEventAggregator.Subscribe( this );

			LoadBands();

			PlayState = ePlayState.StoppedEmptyQueue.ToString();
			mPlayStateChangeDisposable = mPlayController.PlayStateChange.Subscribe( OnPlayStateChange );

			IsActive = true; // default to the active state.
		}

        public bool	IsActive {
            get => mIsActive;
            set {
				mIsActive = value;

				if( IsActive ) {
					RaisePropertyChanged( () => AudioDeviceAvailable );
                }
            }
        }


		private void OnPlayStateChange( ePlayState state ) {
			PlayState = state.ToString();

			RaisePropertyChanged( () => PlayState );
		}

		public string PlayState {
			get {
#if DEBUG
				return Get( () => PlayState );
#else
				return string.Empty;
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

		public ePlaybackStatus CurrentStatus {
			get { return( Get(() => CurrentStatus, ePlaybackStatus.Stopped ));  }
			set { Set(() => CurrentStatus, value ); }
		}

		public int StartTrackFlag {
			get{ return( Get( () => StartTrackFlag, 0 )); }
			set{ Set( () => StartTrackFlag, value  ); }
		}

		public int InfoUpdateFlag {
			get{ return( Get( () => InfoUpdateFlag, 0 ));  }
			set{ Execute.OnUIThread( () => Set( () => InfoUpdateFlag, value )); }
		}

		public int AudioParametersFlag {
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

				return retValue;
			} 
		}

		[DependsUpon( "StartTrackFlag")]
		public string ArtistName {
			get {
				var retValue = string.Empty;

				if( mPlayQueue.PlayingTrack != null ) {
					retValue = mPlayQueue.PlayingTrack.IsStream ? mPlayQueue.PlayingTrack.StreamInfo.Artist : mPlayQueue.PlayingTrack.Artist.Name;
				}

				return retValue;
			}
		}

		[DependsUpon( "StartTrackFlag" )]
		public string AlbumName {
			get {
				var retValue = string.Empty;

				if( mPlayQueue.PlayingTrack != null ) {
					var track = mPlayQueue.PlayingTrack;

					retValue = track.IsStream ? track.StreamInfo != null ? track.StreamInfo.Album : track.Stream.Description : track.Album.Name;
				}

				return retValue;
			}
		}

		[DependsUpon( "StartTrackFlag" )]
		public string ArtistAlbumName {
			get {
				var retValue = string.Empty;

				if( mPlayQueue.PlayingTrack != null ) {
					var track = mPlayQueue.PlayingTrack;

					retValue = track.IsStream ? 
                        track.StreamInfo != null ? $" ({track.StreamInfo.Artist}/{track.StreamInfo.Album})" : 
                                                   $" - {track.Stream.Description}" : 
                                                   $" ({track.Artist.Name}/{track.Album.Name})";
				}

				return retValue;
			}
		}

		[DependsUpon( "StartTrackFlag" )]
		public PlayQueueTrack PeekNextTrack => mPlayController.NextTrack;

        [DependsUpon( "StartTrackFlag" )]
		public PlayQueueTrack PeekPreviousTrack => mPlayController.PreviousTrack;

        [DependsUpon( "InfoUpdateFlag" )]
		public TimeSpan LeftTrackTime => mPlayController.LeftTrackTime;

        [DependsUpon( "InfoUpdateFlag" )]
        public bool IsLeftTrackTimeActive => mPlayController.IsLeftTrackTimeActive;

        [DependsUpon( "InfoUpdateFlag" )]
        public TimeSpan RightTrackTime => mPlayController.RightTrackTime;

        [DependsUpon( "InfoUpdateFlag" )]
        public bool IsRightTrackTimeActive => mPlayController.IsRightTrackTimeActive;

        [DependsUpon( "InfoUpdateFlag" )]
		public double PlayPositionPercentage => mPlayController.PlayPositionPercentage;

        [DependsUpon( "InfoUpdateFlag" )]
		public double PlayPositionPercentagePlus => PlayPositionPercentage > 0.0D ? PlayPositionPercentage + 0.035D : 0.0D;

        [DependsUpon( "InfoUpdateFlag" )]
		public long TrackPosition {
			get => mPlayController.PlayPosition;
            set => mPlayController.PlayPosition = value;
        }

		public void Execute_ToggleTimeDisplay() {
			mPlayController.ToggleTimeDisplay();
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public long TrackEndPosition => mPlayController.TrackEndPosition;

        [DependsUpon( "InfoUpdateFlag" )]
		public double Volume {
			get => mAudioController.Volume;
            set => mAudioController.Volume = (float)value;
        }

		[DependsUpon( "InfoUpdateFlag" )]
		public bool IsMuted => mAudioController.Mute;

        public void Execute_Mute() {
			mAudioController.Mute = !mAudioController.Mute;
		}

		[DependsUpon( "InfoUpdateFlag" )]
		[DependsUpon( "AudioParametersFlag" )]
		public double PlaySpeed {
			get => mAudioController.PlaySpeed;
            set => mAudioController.PlaySpeed = (float)value;
        }

		public void Execute_ResetPlaySpeed() {
			mAudioController.SetDefaultPlaySpeed();
		}

		[DependsUpon( "InfoUpdateFlag" )]
		[DependsUpon( "AudioParametersFlag" )]
		public double PanPosition {
			get => mAudioController.PanPosition;
            set => mAudioController.PanPosition = (float)value;
        }

		public void Execute_ResetPanPosition() {
			mAudioController.SetDefaultPanPosition();
		}

		[DependsUpon( "InfoUpdateFlag" )]
		public double LeftLevel => mPlayController.LeftLevel;

        [DependsUpon( "InfoUpdateFlag" )]
		public double RightLevel => mPlayController.RightLevel;

        [DependsUpon( "StartTrackFlag" )]
		[DependsUpon( "InfoUpdateFlag" )]
		public bool IsFavorite {
			get => mPlayController.IsFavorite;
            set => mPlayController.IsFavorite = value;
        }

		[DependsUpon( "StartTrackFlag" )]
		[DependsUpon( "InfoUpdateFlag" )]
		public Int16 Rating {
			get => mPlayController.Rating;
            set => mPlayController.Rating = value;
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
			get => mAudioController.TrackOverlapEnable;
            set => mAudioController.TrackOverlapEnable = value;
        }

		[DependsUpon( "AudioParametersFlag" )]
		public int TrackOverlapMilliseconds {
			get => mAudioController.TrackOverlapMilliseconds;
            set => mAudioController.TrackOverlapMilliseconds = value;
        }

		public void Execute_PlayerSwitch() {
			mEventAggregator.PublishOnUIThread( new Events.ExternalPlayerSwitch());
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
			get => mSpectrumImageHeight;
            set {
				if((!double.IsNaN( value )) &&
				   ( value >  0 )) {
					mSpectrumImageHeight = value;
				}
			}
		}

		public double ImageWidth {
			get => mSpectrumImageWidth;
            set{
				if((!double.IsNaN( value )) &&
				   ( value > 0 )) {
					mSpectrumImageWidth = value; 
				}
			}
		}

		public IEnumerable<AudioDevice>	AudioDevices => mAudioController.AudioDevices;
		public bool AudioDeviceAvailable => CurrentAudioDevice?.WillMakeNoise == true;

        public AudioDevice CurrentAudioDevice {
			get => mAudioController.CurrentAudioDevice;
            set {
				mPlayController.Stop();
				mAudioController.CurrentAudioDevice = value; 
				
				RaisePropertyChanged( () => CurrentAudioDevice );
				RaisePropertyChanged( () => AudioDeviceAvailable );
			}
		}


		[DependsUpon( "AudioParametersFlag" )]
		public double PreampVolume {
			get => mAudioController.PreampVolume;
            set {
				if( Math.Abs( mAudioController.PreampVolume - value ) > 0.01D ) {
					mAudioController.PreampVolume = value;

					RaisePropertyChanged( () => PreampVolume );
				}
			}
		}

		[DependsUpon( "AudioParametersFlag" )]
		public bool ReplayGainEnabled {
			get => mPlayController.ReplayGainEnable;
            set => mPlayController.ReplayGainEnable = value;
        }

		public List<ParametricEqualizer> EqualizerList => new List<ParametricEqualizer>( from ParametricEqualizer eq in mAudioController.EqManager.EqPresets orderby eq.Name select eq );

        public ParametricEqualizer CurrentEq {
			get => mAudioController.CurrentEq;
            set {
				mAudioController.CurrentEq = value;
				Set( () => CurrentEq, value );

				LoadBands();
			}
		}

		[DependsUpon( "CurrentEq" )]
		public bool EqEnabled {
			get => mAudioController.EqEnabled;
            set => mAudioController.EqEnabled = value;
        }

		[DependsUpon( "CurrentEq" )]
		public ObservableCollection<UiEqBand> EqualizerBands => mBands;

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
			get {
                if( CurrentEq != null ) {
                    return !CurrentEq.IsPreset;
                }

                return false;
            }
		}

		[DependsUpon( "CurrentEq" )]
		public bool CanExecute_ResetBands() {
			return( IsEqEditable );
		}

		[DependsUpon( "AudioParametersFlag" )]
		public bool StereoEnhancerEnable {
			get => mAudioController.StereoEnhancerEnable;
            set {
				mAudioController.StereoEnhancerEnable = value;

				RaisePropertyChanged( () => StereoEnhancerEnable );
			}
		}

		[DependsUpon( "AudioParametersFlag" )]
		public double StereoEnhancerWidth {
			get => mAudioController.StereoEnhancerWidth;
            set => mAudioController.StereoEnhancerWidth = value;
        }

		[DependsUpon( "AudioParametersFlag" )]
		public double StereoEnhancerWetDry {
			get => mAudioController.StereoEnhancerWetDry;
            set => mAudioController.StereoEnhancerWetDry = value;
        }

		[DependsUpon( "AudioParametersFlag" )]
		public bool SoftSaturationEnable {
			get => mAudioController.SoftSaturationEnable;
            set {
				mAudioController.SoftSaturationEnable = value;

				RaisePropertyChanged( () => SoftSaturationEnable );
			}
		}

		[DependsUpon( "AudioParametersFlag" )]
		public double SoftSaturationDepth {
			get => mAudioController.SoftSaturationDepth;
            set => mAudioController.SoftSaturationDepth = value;
        }

		[DependsUpon( "AudioParametersFlag" )]
		public double SoftSaturationFactor {
			get => mAudioController.SoftSaturationFactor;
            set => mAudioController.SoftSaturationFactor = value;
        }

		[DependsUpon( "AudioParametersFlag" )]
		public bool ReverbEnable {
			get => mAudioController.ReverbEnable;
            set {
				mAudioController.ReverbEnable = value;

				RaisePropertyChanged( () => ReverbEnable );
			}
		}

		[DependsUpon( "AudioParametersFlag" )]
		public float ReverbLevel {
			get => mAudioController.ReverbLevel;
            set => mAudioController.ReverbLevel = value;
        }

		[DependsUpon( "AudioParametersFlag" )]
		public float ReverbDelay {
			get => mAudioController.ReverbDelay;
            set => mAudioController.ReverbDelay = value;
        }

		public void Execute_StandardPlayer() {
			mEventAggregator.PublishOnUIThread( new Events.StandardPlayerRequest());
		}

		public void Execute_ExtendedPlayer() {
			mEventAggregator.PublishOnUIThread( new Events.ExtendedPlayerRequest());
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
				mEventAggregator.PublishOnUIThread( new Events.SongLyricsRequest( mLyricsInfo ));
			}
		}

		public bool CanExecute_RequestLyrics() {
			return( mLyricsInfo != null );
		}

		public void Execute_TrackInformation() {
			if( mPlayController.CurrentTrack.Album != null ) {
				mEventAggregator.PublishOnUIThread( new Events.AlbumFocusRequested( mPlayController.CurrentTrack.Album ));
			}
		}

		[DependsUpon("StartTrackFlag")]
		public bool CanExecute_TrackInformation() {
			return(( mPlayController.CurrentTrack != null ) &&
			       ( mPlayController.CurrentTrack.Album != null ));
		}

		public void Execute_ManagePlaybackContext() {
			if( mPlayController.CurrentTrack != null ) {
				var parameters = new DialogParameters {
                    { PlaybackContextDialogManager.cTrackParameter, mPlayController.CurrentTrack.Track },
					{ PlaybackContextDialogManager.cAlbumParameter, mPlayController.CurrentTrack.Album }};

				mDialogService.ShowDialog( nameof( PlaybackContextDialog ), parameters, result => { });
			}
		}

		[DependsUpon("StartTrackFlag")]
		public bool CanExecute_ManagePlaybackContext() {
			return(( mDialogService != null ) && 
				   ( mPlayController.CurrentTrack != null ));
		}

		public void Execute_DefinePlayPoints() {
			if(( mDialogService != null ) &&
			   ( mPlayController.CurrentTrack != null )) {
				var parameters = new DialogParameters {
                    { TrackPlayPointsDialogModel.cTrackParameter, mPlayController.CurrentTrack.Track },
					{ TrackPlayPointsDialogModel.cCurrentPosition, mPlayController.PlayPosition },
					{ TrackPlayPointsDialogModel.cTrackLength, mPlayController.TrackEndPosition }};
				mDialogService.ShowDialog( nameof( TrackPlayPointsDialog ), parameters, result => { });
            }
        }
        [DependsUpon( "CurrentStatus" )]
        [DependsUpon("StartTrackFlag")]
		public bool CanExecute_DefinePlayPoints() {
			return mPlayController.CurrentTrack != null;
        }
	}
}
