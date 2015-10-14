using Noise.Infrastructure.Dto;

namespace Noise.Core.PlaySupport {
	internal class PlaybackContext : ScPlayContext {
		public double	PreviousPanPosition { get; set; }
		public double	PreviousPreampVolume { get; set; }
		public double	PreviousPlaySpeed { get; set; }
		public float	PreviousReverbLevel { get; set; }
		public float	PreviousReverbDelay { get; set; }
		public double	PreviousSoftSaturationFactor { get; set; }
		public double	PreviousSoftSaturationDepth { get; set; }
		public double	PreviousStereoEnhancerWidth { get; set; }
		public double	PreviousStereoEnhancerWetDry { get; set; }
		public int		PreviousTrackOverlap { get; set; }

		public void CombineContext( ScPlayContext context ) {
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
					ReverbDelay = context.ReverbDelay;
					ReverbLevel = context.ReverbLevel;
				}

				if( context.SoftSaturationValid ) {
					SoftSaturationValid = true;
					SoftSaturationDepth = context.SoftSaturationDepth;
					SoftSaturationFactor = context.SoftSaturationFactor;
				}

				if( context.StereoEnhancerValid ) {
					StereoEnhancerValid = true;
					StereoEnhancerWetDry = context.StereoEnhancerWetDry;
					StereoEnhancerWidth = context.StereoEnhancerWidth;
				}

				if( context.TrackOverlapValid ) {
					TrackOverlapValid = true;
					TrackOverlapMilliseconds = context.TrackOverlapMilliseconds;
				}
			}
		}
	}
}
