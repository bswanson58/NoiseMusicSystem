using Noise.Infrastructure.Dto;

namespace Noise.Core.PlaySupport {
	internal class PlaybackContext : ScPlayContext {
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
	}
}
