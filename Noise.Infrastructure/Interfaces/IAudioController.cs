using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IAudioController {
		IEqManager			EqManager { get; }
		ParametricEqualizer	CurrentEq { get; set; }
		bool				EqEnabled { get; set; }
		void				SetEqValue( long bandId, float gain );

		double				Volume { get; set; }
		bool				Mute { get; set; }
		double				PreampVolume { get; set; }

		double				PanPosition { get; set; }
		void				SetDefaultPanPosition();
		double				PlaySpeed { get; set; }
		void				SetDefaultPlaySpeed();

		bool				ReverbEnable { get; set; }
		float				ReverbLevel { get; set; }
		int					ReverbDelay { get; set; }

		bool				SoftSaturationEnable { get; set; }
		double				SoftSaturationFactor { get; set; }
		double				SoftSaturationDepth { get; set; }

		bool				StereoEnhancerEnable { get; set; }
		double				StereoEnhancerWidth { get; set; }
		double				StereoEnhancerWetDry { get; set; }

		bool				TrackOverlapEnable { get; set; }
		int					TrackOverlapMilliseconds { get; set; }
	}
}
