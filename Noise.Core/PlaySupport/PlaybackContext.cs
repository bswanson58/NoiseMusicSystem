﻿using Noise.Infrastructure.Dto;

namespace Noise.Core.PlaySupport {
	internal class PlaybackContext : ScPlayContext {
		public double	PreviousPanPosition { get; set; }
		public double	PreviousPreampVolume { get; set; }
		public double	PreviousPlaySpeed { get; set; }
		public bool		PreviousReverbEnabled { get; set; }
		public float	PreviousReverbLevel { get; set; }
		public float	PreviousReverbDelay { get; set; }
		public bool		PreviousSoftSaturationEnabled { get; set; }
		public double	PreviousSoftSaturationFactor { get; set; }
		public double	PreviousSoftSaturationDepth { get; set; }
		public bool		PreviousStereoEnhancerEnabled { get; set; }
		public double	PreviousStereoEnhancerWidth { get; set; }
		public double	PreviousStereoEnhancerWetDry { get; set; }
		public bool		PreviousTrackOverlapEnabled { get; set; }
		public int		PreviousTrackOverlap { get; set; }

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
				PanPosition = currentContext.PreviousPanPosition;
			}
			if( newContext.PanPositionValid ) {
				PanPosition = newContext.PanPosition;
			}

			PlaySpeedValid = currentContext.PlaySpeedValid || newContext.PlaySpeedValid;
			if( currentContext.PlaySpeedValid ) {
				PlaySpeed = currentContext.PreviousPlaySpeed;
			}
			if( newContext.PlaySpeedValid ) {
				PlaySpeed = newContext.PlaySpeed;
			}

			PreampVolumeValid = currentContext.PreampVolumeValid || newContext.PreampVolumeValid;
			if( currentContext.PreampVolumeValid ) {
				PreampVolume = currentContext.PreviousPreampVolume;
			}
			if( newContext.PreampVolumeValid ) {
				PreampVolume = newContext.PreampVolume;
			}

			ReverbValid = currentContext.ReverbValid || newContext.ReverbValid;
			if( currentContext.ReverbValid ) {
				ReverbEnabled = currentContext.PreviousReverbEnabled;
				ReverbDelay = currentContext.PreviousReverbDelay;
				ReverbLevel = currentContext.PreviousReverbLevel;
			}
			if(newContext.ReverbValid) {
				ReverbEnabled = newContext.ReverbEnabled;
				ReverbDelay = newContext.ReverbDelay;
				ReverbLevel = newContext.ReverbLevel;
			}

			SoftSaturationValid = currentContext.SoftSaturationValid || newContext.SoftSaturationValid;
			if( currentContext.SoftSaturationValid ) {
				SoftSaturationEnabled = currentContext.PreviousSoftSaturationEnabled;
				SoftSaturationDepth = currentContext.PreviousSoftSaturationDepth;
				SoftSaturationFactor = currentContext.PreviousSoftSaturationFactor;
			}
			if( newContext.SoftSaturationValid ) {
				SoftSaturationEnabled = newContext.SoftSaturationEnabled;
				SoftSaturationDepth = newContext.SoftSaturationDepth;
				SoftSaturationFactor = newContext.SoftSaturationFactor; 
			}

			StereoEnhancerValid = currentContext.StereoEnhancerValid || newContext.StereoEnhancerValid;
			if( currentContext.StereoEnhancerValid ) {
				StereoEnhancerEnabled = currentContext.PreviousStereoEnhancerEnabled;
				StereoEnhancerWetDry = currentContext.PreviousStereoEnhancerWetDry;
				StereoEnhancerWidth = currentContext.PreviousStereoEnhancerWidth;
			}
			if( newContext.StereoEnhancerValid ) {
				StereoEnhancerEnabled = newContext.StereoEnhancerEnabled;
				StereoEnhancerWetDry = newContext.StereoEnhancerWetDry;
				StereoEnhancerWidth = newContext.StereoEnhancerWidth;
			}

			TrackOverlapValid = currentContext.TrackOverlapValid || newContext.TrackOverlapValid;
			if( currentContext.TrackOverlapValid ) {
				TrackOverlapEnabled = currentContext.PreviousTrackOverlapEnabled;
				TrackOverlapMilliseconds = currentContext.PreviousTrackOverlap;
			}
			if( newContext.TrackOverlapValid ) {
				TrackOverlapEnabled = newContext.TrackOverlapEnabled;
				TrackOverlapMilliseconds = newContext.TrackOverlapMilliseconds;
			}
		}
	}
}
