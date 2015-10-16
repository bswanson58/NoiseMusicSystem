using Noise.Infrastructure.Interfaces;

namespace Noise.Infrastructure.Dto {
	public class PlaybackContext : ScPlayContext {
		public void AddContext( ScPlayContext context ) {
			if( context != null ) {
				if( context.PanPositionValid ) {
					PanPositionValid = true;
					PanPosition = context.PanPosition;
				}

				if( context.PlaySpeedValid ) {
					PlaySpeedValid = true;
					PlaySpeed = context.PlaySpeed;
				}

				if( context.PreampVolumeValid ) {
					PreampVolumeValid = true;
					PreampVolume = context.PreampVolume;
				}

				if( context.ReverbValid ) {
					ReverbValid = true;
					ReverbEnabled = context.ReverbEnabled;
					ReverbDelay = context.ReverbDelay;
					ReverbLevel = context.ReverbLevel;
				}

				if( context.SoftSaturationValid ) {
					SoftSaturationValid = true;
					SoftSaturationEnabled = context.SoftSaturationEnabled;
					SoftSaturationDepth = context.SoftSaturationDepth;
					SoftSaturationFactor = context.SoftSaturationFactor;
				}

				if( context.StereoEnhancerValid ) {
					StereoEnhancerValid = true;
					StereoEnhancerEnabled = context.StereoEnhancerEnabled;
					StereoEnhancerWetDry = context.StereoEnhancerWetDry;
					StereoEnhancerWidth = context.StereoEnhancerWidth;
				}

				if( context.TrackOverlapValid ) {
					TrackOverlapValid = true;
					TrackOverlapEnabled = context.TrackOverlapEnabled;
					TrackOverlapMilliseconds = context.TrackOverlapMilliseconds;
				}
			}
		}

		public void CombineContext( PlaybackContext currentContext, PlaybackContext newContext ) {
			PanPositionValid = currentContext.PanPositionValid || newContext.PanPositionValid;
			if( currentContext.PanPositionValid ) {
				PanPosition = currentContext.PanPosition;
			}
			if( newContext.PanPositionValid ) {
				PanPosition = newContext.PanPosition;
			}

			PlaySpeedValid = currentContext.PlaySpeedValid || newContext.PlaySpeedValid;
			if( currentContext.PlaySpeedValid ) {
				PlaySpeed = currentContext.PlaySpeed;
			}
			if( newContext.PlaySpeedValid ) {
				PlaySpeed = newContext.PlaySpeed;
			}

			PreampVolumeValid = currentContext.PreampVolumeValid || newContext.PreampVolumeValid;
			if( currentContext.PreampVolumeValid ) {
				PreampVolume = currentContext.PreampVolume;
			}
			if( newContext.PreampVolumeValid ) {
				PreampVolume = newContext.PreampVolume;
			}

			ReverbValid = currentContext.ReverbValid || newContext.ReverbValid;
			if( currentContext.ReverbValid ) {
				ReverbEnabled = currentContext.ReverbEnabled;
				ReverbDelay = currentContext.ReverbDelay;
				ReverbLevel = currentContext.ReverbLevel;
			}
			if(newContext.ReverbValid) {
				ReverbEnabled = newContext.ReverbEnabled;
				ReverbDelay = newContext.ReverbDelay;
				ReverbLevel = newContext.ReverbLevel;
			}

			SoftSaturationValid = currentContext.SoftSaturationValid || newContext.SoftSaturationValid;
			if( currentContext.SoftSaturationValid ) {
				SoftSaturationEnabled = currentContext.SoftSaturationEnabled;
				SoftSaturationDepth = currentContext.SoftSaturationDepth;
				SoftSaturationFactor = currentContext.SoftSaturationFactor;
			}
			if( newContext.SoftSaturationValid ) {
				SoftSaturationEnabled = newContext.SoftSaturationEnabled;
				SoftSaturationDepth = newContext.SoftSaturationDepth;
				SoftSaturationFactor = newContext.SoftSaturationFactor; 
			}

			StereoEnhancerValid = currentContext.StereoEnhancerValid || newContext.StereoEnhancerValid;
			if( currentContext.StereoEnhancerValid ) {
				StereoEnhancerEnabled = currentContext.StereoEnhancerEnabled;
				StereoEnhancerWetDry = currentContext.StereoEnhancerWetDry;
				StereoEnhancerWidth = currentContext.StereoEnhancerWidth;
			}
			if( newContext.StereoEnhancerValid ) {
				StereoEnhancerEnabled = newContext.StereoEnhancerEnabled;
				StereoEnhancerWetDry = newContext.StereoEnhancerWetDry;
				StereoEnhancerWidth = newContext.StereoEnhancerWidth;
			}

			TrackOverlapValid = currentContext.TrackOverlapValid || newContext.TrackOverlapValid;
			if( currentContext.TrackOverlapValid ) {
				TrackOverlapEnabled = currentContext.TrackOverlapEnabled;
				TrackOverlapMilliseconds = currentContext.TrackOverlapMilliseconds;
			}
			if( newContext.TrackOverlapValid ) {
				TrackOverlapEnabled = newContext.TrackOverlapEnabled;
				TrackOverlapMilliseconds = newContext.TrackOverlapMilliseconds;
			}
		}

		public void ReadAllContext( IAudioController audioController ) {
			PanPositionValid = true;
			PanPosition = audioController.PanPosition;

			PlaySpeedValid = true;
			PlaySpeed = audioController.PlaySpeed;

			PreampVolumeValid = true;
			PreampVolume = audioController.PreampVolume;

			ReverbValid = true;
			ReverbEnabled = audioController.ReverbEnable;
			ReverbDelay = audioController.ReverbDelay;
			ReverbLevel = audioController.ReverbLevel;

			SoftSaturationValid = true;
			SoftSaturationEnabled = audioController.SoftSaturationEnable;
			SoftSaturationDepth = audioController.SoftSaturationDepth;
			SoftSaturationFactor = audioController.SoftSaturationFactor;

			StereoEnhancerValid = true;
			StereoEnhancerEnabled = audioController.StereoEnhancerEnable;
			StereoEnhancerWetDry = audioController.StereoEnhancerWetDry;
			StereoEnhancerWidth = audioController.StereoEnhancerWidth;

			TrackOverlapValid = true;
			TrackOverlapEnabled = audioController.TrackOverlapEnable;
			TrackOverlapMilliseconds = audioController.TrackOverlapMilliseconds;
		}

		public void ReadContext( IAudioController audioController ) {
				PanPosition = PanPositionValid ? audioController.PanPosition : 0.0;
				PlaySpeed = PlaySpeedValid ? audioController.PlaySpeed : 0.0;
				PreampVolume = PreampVolumeValid ? audioController.PreampVolume : 0.0;
				ReverbEnabled = ReverbValid && audioController.ReverbEnable;
				ReverbDelay = ReverbValid ? audioController.ReverbDelay : 0.0f;
				ReverbLevel = ReverbValid ? audioController.ReverbLevel : 0.0f;
				SoftSaturationEnabled = SoftSaturationValid && audioController.SoftSaturationEnable;
				SoftSaturationDepth = SoftSaturationValid ? audioController.SoftSaturationDepth : 0.0;
				SoftSaturationFactor = SoftSaturationValid ? audioController.SoftSaturationFactor : 0.0;
				StereoEnhancerEnabled = StereoEnhancerValid && audioController.StereoEnhancerEnable;
				StereoEnhancerWetDry = StereoEnhancerValid ? audioController.StereoEnhancerWetDry : 0.0;
				StereoEnhancerWidth = StereoEnhancerValid ? audioController.StereoEnhancerWidth : 0.0;
				TrackOverlapEnabled = TrackOverlapValid && audioController.TrackOverlapEnable;
				TrackOverlapMilliseconds = TrackOverlapValid ? audioController.TrackOverlapMilliseconds : 0;
		}

		public void WriteContext( IAudioController audioController ) {
			if( PanPositionValid ) {
				audioController.PanPosition = PanPosition;
			}

			if( PreampVolumeValid ) {
				audioController.PreampVolume = PreampVolume;
			}

			if( PlaySpeedValid ) {
				audioController.PlaySpeed = PlaySpeed;
			}

			if( ReverbValid ) {
				audioController.ReverbDelay = ReverbDelay;
				audioController.ReverbLevel = ReverbLevel;
				audioController.ReverbEnable = ReverbEnabled;
			}

			if( SoftSaturationValid ) {
				audioController.SoftSaturationDepth = SoftSaturationDepth;
				audioController.SoftSaturationFactor = SoftSaturationFactor;
				audioController.SoftSaturationEnable = SoftSaturationEnabled;
			}

			if( StereoEnhancerValid ) {
				audioController.StereoEnhancerWetDry = StereoEnhancerWetDry;
				audioController.StereoEnhancerWidth = StereoEnhancerWidth;
				audioController.StereoEnhancerEnable = StereoEnhancerEnabled;
			}

			if( TrackOverlapValid ) {
				audioController.TrackOverlapMilliseconds = TrackOverlapMilliseconds;
				audioController.TrackOverlapEnable = TrackOverlapEnabled;
			}
		}

		public bool HasContext() {
			return( PanPositionValid || 
					PreampVolumeValid ||
					PlaySpeedValid ||
					ReverbValid ||
					SoftSaturationValid ||
					StereoEnhancerValid ||
					TrackOverlapValid ); 
		}
	}
}
