using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlaySupport {
	public class AudioController : IAudioController, IRequireInitialization,
								   IHandle<Events.SystemShutdown> {
		private readonly IEventAggregator	mEventAggregator;
		private readonly INoiseEnvironment	mNoiseEnvironment;
		private readonly IPreferences		mPreferences;
		private readonly IAudioPlayer		mAudioPlayer;
		private	readonly IEqManager			mEqManager;

		public AudioController( ILifecycleManager lifecycleManager, IEventAggregator eventAggregator, INoiseEnvironment noiseEnvironment,
								IAudioPlayer audioPlayer, IEqManager eqManager, IPreferences preferences ) {
			mEventAggregator = eventAggregator;
			mNoiseEnvironment = noiseEnvironment;
			mPreferences = preferences;
			mAudioPlayer = audioPlayer;
			mEqManager = eqManager;

			lifecycleManager.RegisterForInitialize( this );
		}

		public void Initialize() {
			try {
				var audioCongfiguration = mPreferences.Load<AudioPreferences>();

				if( audioCongfiguration != null ) {
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

					if(!string.IsNullOrWhiteSpace( audioCongfiguration.OutputDevice )) {
						var device = mAudioPlayer.GetDeviceList().FirstOrDefault( d => d.Name.Equals( audioCongfiguration.OutputDevice, StringComparison.InvariantCultureIgnoreCase ));

						if( device != null ) {
							mAudioPlayer.SetDevice( device );
						}
					}
				}

				if( mEqManager.Initialize( Path.Combine( mNoiseEnvironment.ConfigurationDirectory(), Constants.EqPresetsFile ))) {
					mAudioPlayer.ParametricEq = mEqManager.CurrentEq;
					mAudioPlayer.EqEnabled = mEqManager.EqEnabled;
				}
				else {
					NoiseLogger.Current.LogMessage( "EqManager could not be initialized." );
				}

				mEventAggregator.Subscribe( this );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "AudioController:Initialize", ex );
			}
		}

		public void Shutdown() { }

		public void Handle( Events.SystemShutdown eventArgs ) {
			mEventAggregator.Unsubscribe( this );

			var audioCongfiguration = mPreferences.Load<AudioPreferences>();
			if( audioCongfiguration != null ) {
				audioCongfiguration.PreampGain = mAudioPlayer.PreampVolume;
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
				audioCongfiguration.OutputDevice = mAudioPlayer.GetCurrentDevice().Name;

				mPreferences.Save( audioCongfiguration );
			}
		}

		public IEnumerable<AudioDevice> AudioDevices {
			get {  return( mAudioPlayer.GetDeviceList()); }
		}

		public AudioDevice CurrentAudioDevice {
			get{ return( mAudioPlayer.GetCurrentDevice()); }
			set{ mAudioPlayer.SetDevice( value ); }
		}

		public IEqManager EqManager {
			get { return ( mEqManager ); }
		}

		public ParametricEqualizer CurrentEq {
			get { return ( mAudioPlayer.ParametricEq ); }
			set {
				mAudioPlayer.ParametricEq = value;
				mEqManager.CurrentEq = value;
			}
		}

		public double Volume {
			get { return ( mAudioPlayer.Volume ); }
			set {
				mAudioPlayer.Volume = (float)value;

				FirePlaybackInfoChange();
			}
		}

		public bool Mute {
			get { return ( mAudioPlayer.Mute ); }
			set {
				mAudioPlayer.Mute = value;

				FirePlaybackInfoChange();
			}
		}

		public double PreampVolume {
			get { return ( mAudioPlayer.PreampVolume ); }
			set { mAudioPlayer.PreampVolume = (float)value; }
		}

		public double PlaySpeed {
			get { return ( mAudioPlayer.PlaySpeed ); }
			set {
				mAudioPlayer.PlaySpeed = (float)value;

				FirePlaybackInfoChange();
			}
		}

		public void SetDefaultPlaySpeed() {
			PlaySpeed = 0.0;
		}

		public double PanPosition {
			get { return ( mAudioPlayer.Pan ); }
			set {
				mAudioPlayer.Pan = (float)value;

				FirePlaybackInfoChange();
			}
		}

		public void SetDefaultPanPosition() {
			PanPosition = 0.0;
		}

		public bool EqEnabled {
			get { return ( mAudioPlayer.EqEnabled ); }
			set {
				mAudioPlayer.EqEnabled = value;
				mEqManager.EqEnabled = value;
			}
		}

		public void SetEqValue( long bandId, float gain ) {
			mAudioPlayer.AdjustEq( bandId, gain );
		}

		public bool StereoEnhancerEnable {
			get { return ( mAudioPlayer.StereoEnhancerEnable ); }
			set { mAudioPlayer.StereoEnhancerEnable = value; }
		}

		public double StereoEnhancerWidth {
			get { return ( mAudioPlayer.StereoEnhancerWidth ); }
			set { mAudioPlayer.StereoEnhancerWidth = value; }
		}

		public double StereoEnhancerWetDry {
			get { return ( mAudioPlayer.StereoEnhancerWetDry ); }
			set { mAudioPlayer.StereoEnhancerWetDry = value; }
		}

		public bool SoftSaturationEnable {
			get { return ( mAudioPlayer.SoftSaturationEnable ); }
			set { mAudioPlayer.SoftSaturationEnable = value; }
		}

		public double SoftSaturationDepth {
			get { return ( mAudioPlayer.SoftSaturationDepth ); }
			set { mAudioPlayer.SoftSaturationDepth = value; }
		}

		public double SoftSaturationFactor {
			get { return ( mAudioPlayer.SoftSaturationFactor ); }
			set { mAudioPlayer.SoftSaturationFactor = value; }
		}

		public bool ReverbEnable {
			get { return ( mAudioPlayer.ReverbEnable ); }
			set { mAudioPlayer.ReverbEnable = value; }
		}

		public float ReverbLevel {
			get { return ( mAudioPlayer.ReverbLevel ); }
			set { mAudioPlayer.ReverbLevel = value; }
		}

		public float ReverbDelay {
			get { return ( mAudioPlayer.ReverbDelay ); }
			set { mAudioPlayer.ReverbDelay = value; }
		}

		public bool TrackOverlapEnable {
			get { return ( mAudioPlayer.TrackOverlapEnable ); }
			set { mAudioPlayer.TrackOverlapEnable = value; }
		}

		public int TrackOverlapMilliseconds {
			get { return ( mAudioPlayer.TrackOverlapMilliseconds ); }
			set { mAudioPlayer.TrackOverlapMilliseconds = value; }
		}

		private void FirePlaybackInfoChange() {
			mEventAggregator.Publish( new Events.PlaybackInfoChanged() );
		}
	}
}
