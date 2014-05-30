using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Configuration {
	public class AudioPreferences {
		public	long		DefaultEqualizer { get; set; }
		public	float		PreampGain { get; set; }
		public	bool		ReplayGainEnabled { get; set; }
		public	bool		EqEnabled { get; set; }
		public	bool		StereoEnhancerEnabled { get; set; }
		public	double		StereoEnhancerWidth { get; set; }
		public	double		StereoEnhancerWetDry { get; set; }
		public	bool		SoftSaturationEnabled { get; set; }
		public	double		SoftSaturationDepth { get; set; }
		public	double		SoftSaturationFactor { get; set; }
		public	bool		ReverbEnabled { get; set; }
		public	float		ReverbLevel { get; set; }
		public	float		ReverbDelay { get; set; }
		public	bool		TrackOverlapEnabled { get; set; }
		public	int			TrackOverlapMilliseconds { get; set; }
		public	IList<ParametricEqConfiguration> ParametricEqualizers { get; set; }

		public AudioPreferences() {
			ParametricEqualizers = new List<ParametricEqConfiguration>();

			PreampGain = 1.0f;
			ReplayGainEnabled = true;
			StereoEnhancerWidth = 0.2f;
			StereoEnhancerWetDry = 0.5f;
			SoftSaturationDepth = 0.5f;
			SoftSaturationFactor = 0.5f;
			ReverbLevel = 0.3f;
			ReverbDelay = 0.1f;
			TrackOverlapEnabled = true;
			TrackOverlapMilliseconds = 500;
		}

		public void UpdateEq( ParametricEqualizer eq ) {
			var eqConf = ( from ParametricEqConfiguration item in ParametricEqualizers where item.EqId == eq.EqualizerId  select item ).FirstOrDefault();

			if( eqConf == null ) {
				eqConf = new ParametricEqConfiguration { EqId = eq.EqualizerId };

				ParametricEqualizers.Add( eqConf );
			}

			eqConf.Bandwidth = eq.Bandwidth;
			eqConf.EqDescription = eq.Description;
			eqConf.EqName = eq.Name;
			eqConf.IsPreset = eq.IsPreset;

			eqConf.Bands.Clear();
			foreach( var band in eq.Bands ) {
				eqConf.Bands.Add( new ParametricBandConfiguration { BandId = band.BandId, CenterFrequency = band.CenterFrequency, Gain = band.Gain });
			}
		}
	}

	public class ParametricEqConfiguration {
		public	long		EqId { get; set; }
		public	string		EqName { get; set; }
		public	string		EqDescription { get; set; }
		public	float		Bandwidth { get; set; }
		public	bool		IsPreset { get; set; }
		public	IList<ParametricBandConfiguration> Bands { get; set; }

		public ParametricEqConfiguration() {
			Bands = new List<ParametricBandConfiguration>();

			EqName = string.Empty;
			EqDescription = string.Empty;
			Bandwidth = 18.0f;
		}

		public	ParametricEqualizer AsParametericEqualizer() {
			var retValue = new ParametricEqualizer( EqId, IsPreset, EqName, EqDescription, Bandwidth );

			foreach( ParametricBandConfiguration bandConfig in Bands ) {
				retValue.AddBand( new ParametricBand( bandConfig.BandId, bandConfig.CenterFrequency, bandConfig.Gain ));
			}

			return( retValue );
		}
	}

	public class ParametricBandConfiguration {
		public	long	BandId { get; set; }
		public	float	CenterFrequency { get; set; }
		public	float	Gain { get; set; }
	}
}
