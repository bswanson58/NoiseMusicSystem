using Noise.Infrastructure.Dto;

namespace Noise.Core.PlaySupport {
	internal class PlaybackContext : ScPlayContext {
		public double	PreviousPanPosition { get; set; }
		public double	PreviousPreampVolume { get; set; }
		public double	PreviousPlaySpeed { get; set; }

		public bool HasContext {
			get {  return( PanPositionValid || PreampVolumeValid || PlaySpeedValid ); }
		}

		public void CombineContext( ScPlayContext context ) {
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
		}
	}
}
