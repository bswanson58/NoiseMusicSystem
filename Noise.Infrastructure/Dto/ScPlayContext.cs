namespace Noise.Infrastructure.Dto {
	public class ScPlayContext {
		public bool		PanPositionValid { get; set; }
		public double	PanPosition {  get; set; }

		public bool		PreampVolumeValid { get; set; }
		public double	PreampVolume {  get; set; }

		public bool		PlaySpeedValid {  get; set; }
		public double	PlaySpeed { get; set; }
	}
}
