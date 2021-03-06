﻿namespace Noise.Infrastructure.Dto {
	public class ScPlayContext {
		public bool		PanPositionValid { get; set; }
		public double	PanPosition {  get; set; }

		public bool		PreampVolumeValid { get; set; }
		public double	PreampVolume {  get; set; }

		public bool		PlaySpeedValid {  get; set; }
		public double	PlaySpeed { get; set; }

		public bool		ReverbValid { get; set; }
		public bool		ReverbEnabled { get; set; }
		public float	ReverbLevel { get; set; }
		public float	ReverbDelay { get; set; }

		public bool		SoftSaturationValid { get; set; }
		public bool		SoftSaturationEnabled { get; set; }
		public double	SoftSaturationFactor { get; set; }
		public double	SoftSaturationDepth { get; set; }

		public bool		StereoEnhancerValid { get; set; }
		public bool		StereoEnhancerEnabled { get; set; }
		public double	StereoEnhancerWidth { get; set; }
		public double	StereoEnhancerWetDry { get; set; }

		public bool		TrackOverlapValid { get; set; }
		public bool		TrackOverlapEnabled { get; set; }
		public int		TrackOverlapMilliseconds { get; set; }
	}
}
