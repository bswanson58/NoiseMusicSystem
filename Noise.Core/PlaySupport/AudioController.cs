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
		private readonly INoiseLog			mLog;
		private readonly INoiseEnvironment	mNoiseEnvironment;
		private readonly IPreferences		mPreferences;
		private readonly IAudioPlayer		mAudioPlayer;

        public	IEqManager					EqManager { get; }


		public AudioController( ILifecycleManager lifecycleManager, IEventAggregator eventAggregator, INoiseEnvironment noiseEnvironment,
								IAudioPlayer audioPlayer, IEqManager eqManager, IPreferences preferences, INoiseLog log ) {
			mEventAggregator = eventAggregator;
			mNoiseEnvironment = noiseEnvironment;
			mLog = log;
			mPreferences = preferences;
			mAudioPlayer = audioPlayer;
			EqManager = eqManager;

			lifecycleManager.RegisterForInitialize( this );
		}

		public void Initialize() {
			try {
				var audioConfiguration = mPreferences.Load<AudioPreferences>();

				if( audioConfiguration != null ) {
					mAudioPlayer.PreampVolume = audioConfiguration.PreampGain;
					mAudioPlayer.StereoEnhancerEnable = audioConfiguration.StereoEnhancerEnabled;
					mAudioPlayer.StereoEnhancerWidth = audioConfiguration.StereoEnhancerWidth;
					mAudioPlayer.StereoEnhancerWetDry = audioConfiguration.StereoEnhancerWetDry;
					mAudioPlayer.SoftSaturationEnable = audioConfiguration.SoftSaturationEnabled;
					mAudioPlayer.SoftSaturationFactor = audioConfiguration.SoftSaturationFactor;
					mAudioPlayer.SoftSaturationDepth = audioConfiguration.SoftSaturationDepth;
					mAudioPlayer.ReverbEnable = audioConfiguration.ReverbEnabled;
					mAudioPlayer.ReverbDelay = audioConfiguration.ReverbDelay;
					mAudioPlayer.ReverbLevel = audioConfiguration.ReverbLevel;
					mAudioPlayer.TrackOverlapEnable = audioConfiguration.TrackOverlapEnabled;
					mAudioPlayer.TrackOverlapMilliseconds = audioConfiguration.TrackOverlapMilliseconds;

					var deviceList = mAudioPlayer.GetDeviceList().ToArray();
					if( deviceList.Any()) {
						var device = default( AudioDevice );

						if(!string.IsNullOrWhiteSpace( audioConfiguration.OutputDevice )) {
							device = deviceList.FirstOrDefault( d => d.Name.Equals( audioConfiguration.OutputDevice, StringComparison.InvariantCultureIgnoreCase ));
						}

						if( device == null ) {
							device = deviceList.FirstOrDefault();
						}

						mAudioPlayer.SetDevice( device );
					}
				}

				if( EqManager.Initialize( Path.Combine( mNoiseEnvironment.ConfigurationDirectory(), Constants.EqPresetsFile ))) {
					mAudioPlayer.ParametricEq = EqManager.CurrentEq;
					mAudioPlayer.EqEnabled = EqManager.EqEnabled;
				}
				else {
					mLog.LogMessage( "EqManager could not be initialized." );
				}

				mEventAggregator.Subscribe( this );
			}
			catch( Exception ex ) {
				mLog.LogException( "Initializing audio controller", ex );
			}
		}

		public void Shutdown() { }

		public void Handle( Events.SystemShutdown eventArgs ) {
			mEventAggregator.Unsubscribe( this );

			var audioConfiguration = mPreferences.Load<AudioPreferences>();
			if( audioConfiguration != null ) {
				audioConfiguration.PreampGain = mAudioPlayer.PreampVolume;
				audioConfiguration.StereoEnhancerEnabled = mAudioPlayer.StereoEnhancerEnable;
				audioConfiguration.StereoEnhancerWidth = mAudioPlayer.StereoEnhancerWidth;
				audioConfiguration.StereoEnhancerWetDry = mAudioPlayer.StereoEnhancerWetDry;
				audioConfiguration.SoftSaturationEnabled = mAudioPlayer.SoftSaturationEnable;
				audioConfiguration.SoftSaturationFactor = mAudioPlayer.SoftSaturationFactor;
				audioConfiguration.SoftSaturationDepth = mAudioPlayer.SoftSaturationDepth;
				audioConfiguration.ReverbEnabled = mAudioPlayer.ReverbEnable;
				audioConfiguration.ReverbDelay = mAudioPlayer.ReverbDelay;
				audioConfiguration.ReverbLevel = mAudioPlayer.ReverbLevel;
				audioConfiguration.TrackOverlapEnabled = mAudioPlayer.TrackOverlapEnable;
				audioConfiguration.TrackOverlapMilliseconds = mAudioPlayer.TrackOverlapMilliseconds;
				audioConfiguration.OutputDevice = mAudioPlayer.GetCurrentDevice().Name;

				mPreferences.Save( audioConfiguration );
			}
		}

		public IEnumerable<AudioDevice> AudioDevices => mAudioPlayer.GetDeviceList();

        public AudioDevice CurrentAudioDevice {
			get => ( mAudioPlayer.GetCurrentDevice());
            set => mAudioPlayer.SetDevice( value );
        }

		public ParametricEqualizer CurrentEq {
			get => mAudioPlayer.ParametricEq;
            set {
				mAudioPlayer.ParametricEq = value;
				EqManager.CurrentEq = value;
			}
		}

		public double Volume {
			get => mAudioPlayer.Volume;
            set {
				mAudioPlayer.Volume = (float)value;

				FirePlaybackInfoChange();
			}
		}

		public bool Mute {
			get => mAudioPlayer.Mute;
            set {
				mAudioPlayer.Mute = value;

				FirePlaybackInfoChange();
			}
		}

		public double PreampVolume {
			get => mAudioPlayer.PreampVolume;
            set => mAudioPlayer.PreampVolume = (float)value;
        }

		public double PlaySpeed {
			get => mAudioPlayer.PlaySpeed;
            set {
				mAudioPlayer.PlaySpeed = (float)value;

				FirePlaybackInfoChange();
			}
		}

		public void SetDefaultPlaySpeed() {
			PlaySpeed = 0.0;
		}

		public double PanPosition {
			get => mAudioPlayer.Pan;
            set {
				mAudioPlayer.Pan = (float)value;

				FirePlaybackInfoChange();
			}
		}

		public void SetDefaultPanPosition() {
			PanPosition = 0.0;
		}

		public bool EqEnabled {
			get => mAudioPlayer.EqEnabled;
            set {
				mAudioPlayer.EqEnabled = value;
				EqManager.EqEnabled = value;
			}
		}

		public void SetEqValue( long bandId, float gain ) {
			mAudioPlayer.AdjustEq( bandId, gain );
		}

		public bool StereoEnhancerEnable {
			get => mAudioPlayer.StereoEnhancerEnable;
            set => mAudioPlayer.StereoEnhancerEnable = value;
        }

		public double StereoEnhancerWidth {
			get => mAudioPlayer.StereoEnhancerWidth;
            set => mAudioPlayer.StereoEnhancerWidth = value;
        }

		public double StereoEnhancerWetDry {
			get => mAudioPlayer.StereoEnhancerWetDry;
            set => mAudioPlayer.StereoEnhancerWetDry = value;
        }

		public bool SoftSaturationEnable {
			get => mAudioPlayer.SoftSaturationEnable;
            set => mAudioPlayer.SoftSaturationEnable = value;
        }

		public double SoftSaturationDepth {
			get => mAudioPlayer.SoftSaturationDepth;
            set => mAudioPlayer.SoftSaturationDepth = value;
        }

		public double SoftSaturationFactor {
			get => mAudioPlayer.SoftSaturationFactor;
            set => mAudioPlayer.SoftSaturationFactor = value;
        }

		public bool ReverbEnable {
			get => mAudioPlayer.ReverbEnable;
            set => mAudioPlayer.ReverbEnable = value;
        }

		public float ReverbLevel {
			get => mAudioPlayer.ReverbLevel;
            set => mAudioPlayer.ReverbLevel = value;
        }

		public float ReverbDelay {
			get => mAudioPlayer.ReverbDelay;
            set => mAudioPlayer.ReverbDelay = value;
        }

		public bool TrackOverlapEnable {
			get => mAudioPlayer.TrackOverlapEnable;
            set => mAudioPlayer.TrackOverlapEnable = value;
        }

		public int TrackOverlapMilliseconds {
			get => mAudioPlayer.TrackOverlapMilliseconds;
            set => mAudioPlayer.TrackOverlapMilliseconds = value;
        }

		private void FirePlaybackInfoChange() {
			mEventAggregator.PublishOnUIThread( new Events.PlaybackInfoChanged() );
		}
	}
}
