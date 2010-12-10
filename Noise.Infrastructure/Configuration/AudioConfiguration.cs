using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Configuration {
	public class AudioConfiguration : ConfigurationSection {
		public	const string	SectionName = "audioConfiguration";

		private const string	cParametricEqualizersProperty	= "parametricEqualizers";
		private const string	cDefaultEqualizerProperty		= "defaultEq";
		private const string	cEqualizationEnabledProperty	= "eqEnabled";
		private const string	cPreampGainProperty				= "preampGain";
		private	const string	cEnableReplayGainProperty		= "replayGainEnabled";
		private const string	cEnableStereoEnhancerProperty	= "enableStereoEnhancer";
		private	const string	cStereoEnhancerWidthProperty	= "stereoEnhancerWidth";
		private const string	cStereoEnhancerWetDryProperty	= "stereoEnhancerWetDry";
		private const string	cEnableSoftSaturationPropery	= "enableSoftSaturation";
		private const string	cSoftSaturationDepthProperty	= "softSaturationDepth";
		private const string	cSoftSaturationFactorProperty	= "softSaturationFactor";
		private const string	cEnableReverbProperty			= "enableReverb";
		private const string	cReverbLevelProperty			= "reverbLevel";
		private const string	cReverbDelayProperty			= "reverbDelay";
		private const string	cEnableTrackOverlapProperty		= "enableTrackOverlap";
		private const string	cTrackOverlapProperty			= "trackOverlapMilliseconds";

		public override bool IsReadOnly() {
			return( false );
		}

		[ConfigurationPropertyAttribute( cDefaultEqualizerProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "0" )]
		public long DefaultEqualizer {
			get { return ((long)( base[cDefaultEqualizerProperty])); }
			set { base[cDefaultEqualizerProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cPreampGainProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "1.0" )]
		public float PreampGain {
			get { return ((float)( base[cPreampGainProperty])); }
			set { base[cPreampGainProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cEnableReplayGainProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "true" )]
		public bool ReplayGainEnabled {
			get { return ((bool)( base[cEnableReplayGainProperty])); }
			set { base[cEnableReplayGainProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cEqualizationEnabledProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "false" )]
		public bool EqEnabled {
			get { return ((bool)( base[cEqualizationEnabledProperty])); }
			set { base[cEqualizationEnabledProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cEnableStereoEnhancerProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "false" )]
		public bool StereoEnhancerEnabled {
			get { return ((bool)( base[cEnableStereoEnhancerProperty])); }
			set { base[cEnableStereoEnhancerProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cStereoEnhancerWidthProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "0.2" )]
		public double StereoEnhancerWidth {
			get { return ((double)( base[cStereoEnhancerWidthProperty])); }
			set { base[cStereoEnhancerWidthProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cStereoEnhancerWetDryProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "0.5" )]
		public double StereoEnhancerWetDry {
			get { return ((double)( base[cStereoEnhancerWetDryProperty])); }
			set { base[cStereoEnhancerWetDryProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cEnableSoftSaturationPropery, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "false" )]
		public bool SoftSaturationEnabled {
			get { return ((bool)( base[cEnableSoftSaturationPropery])); }
			set { base[cEnableSoftSaturationPropery] = value; }
		}

		[ConfigurationPropertyAttribute( cSoftSaturationDepthProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "0.5" )]
		public double SoftSaturationDepth {
			get { return ((double)( base[cSoftSaturationDepthProperty])); }
			set { base[cSoftSaturationDepthProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cSoftSaturationFactorProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "0.5" )]
		public double SoftSaturationFactor {
			get { return ((double)( base[cSoftSaturationFactorProperty])); }
			set { base[cSoftSaturationFactorProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cEnableReverbProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "false" )]
		public bool ReverbEnabled {
			get { return ((bool)( base[cEnableReverbProperty])); }
			set { base[cEnableReverbProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cReverbLevelProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "0.0" )]
		public float ReverbLevel {
			get { return ((float)( base[cReverbLevelProperty])); }
			set { base[cReverbLevelProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cReverbDelayProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "1200" )]
		public int ReverbDelay {
			get { return ((int)( base[cReverbDelayProperty])); }
			set { base[cReverbDelayProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cEnableTrackOverlapProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "false" )]
		public bool TrackOverlapEnabled {
			get { return ((bool)( base[cEnableTrackOverlapProperty])); }
			set { base[cEnableTrackOverlapProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cTrackOverlapProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "50" )]
		public int TrackOverlapMilliseconds {
			get { return ((int)( base[cTrackOverlapProperty])); }
			set { base[cTrackOverlapProperty] = value; }
		}

		[ConfigurationProperty( cParametricEqualizersProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false )]
		public ConfigurationElementCollection<ParametricEqConfiguration> ParametricEqualizers {
			get { return((ConfigurationElementCollection<ParametricEqConfiguration>)( base[cParametricEqualizersProperty])); }
			set { base[cParametricEqualizersProperty] = value; }
		}

		public void UpdateEq( ParametricEqualizer eq ) {
			var eqConf = ( from ParametricEqConfiguration item in ParametricEqualizers where item.EqId == eq.EqualizerId  select item ).FirstOrDefault();

			if( eqConf == null ) {
				eqConf = new ParametricEqConfiguration { EqId = eq.EqualizerId };

				ParametricEqualizers.Add( eqConf );
			}
			else {
				var eqList = new List<ParametricBandConfiguration>( eqConf.Bands.OfType<ParametricBandConfiguration>().ToArray());

				foreach( var bandConfig in eqList ) {
					eqConf.Bands.Remove( bandConfig );
				}
			}

			eqConf.Bandwidth = eq.Bandwidth;
			eqConf.EqDescription = eq.Description;
			eqConf.EqName = eq.Name;
			eqConf.IsPreset = eq.IsPreset;

			foreach( var band in eq.Bands ) {
				eqConf.Bands.Add( new ParametricBandConfiguration { BandId = band.BandId, CenterFrequency = band.CenterFrequency, Gain = band.Gain });
			}
		}
	}

	public class ParametricEqConfiguration : ConfigurationElement {
		private const string	cEqIdProperty			= "eqId";
		private const string	cEqNameProperty			= "eqName";
		private const string	cEqDescriptionProperty	= "eqDescription";
		private const string	cEqBandwidthProperty	= "eqBandwidth";
		private const string	cEqBandsProperty		= "eqBands";
		private const string	cEqIsPresetProperty		= "eqIsPreset";

		[ConfigurationPropertyAttribute( cEqIdProperty, IsRequired = true, IsKey = true, IsDefaultCollection = false )]
		public long EqId {
			get { return ((long)( base[cEqIdProperty])); }
			set { base[cEqIdProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cEqNameProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "" )]
		public string EqName {
			get { return ((string)( base[cEqNameProperty])); }
			set { base[cEqNameProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cEqDescriptionProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "" )]
		public string EqDescription {
			get { return ((string)( base[cEqDescriptionProperty] ) ); }
			set { base[cEqDescriptionProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cEqBandwidthProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "18.0" )]
		public float Bandwidth {
			get { return ((float)( base[cEqBandwidthProperty] ) ); }
			set { base[cEqBandwidthProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cEqIsPresetProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "false" )]
		public bool IsPreset {
			get { return ((bool)( base[cEqIsPresetProperty] ) ); }
			set { base[cEqIsPresetProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cEqBandsProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false )]
		public ConfigurationElementCollection<ParametricBandConfiguration> Bands {
			get { return ((ConfigurationElementCollection<ParametricBandConfiguration>)( base[cEqBandsProperty] ) ); }
			set { base[cEqBandsProperty] = value; }
		}

		public ParametricEqualizer AsParametericEqualizer() {
			var retValue = new ParametricEqualizer( EqId, IsPreset, EqName, EqDescription, Bandwidth );

			foreach( ParametricBandConfiguration bandConfig in Bands ) {
				retValue.AddBand( new ParametricBand( bandConfig.BandId, bandConfig.CenterFrequency, bandConfig.Gain ));
			}

			return( retValue );
		}

		public override string ToString() {
			return( EqId.ToString());
		}
	}

	public class ParametricBandConfiguration : ConfigurationElement {
		private const string	cEqBandIdProperty				= "bandId";
		private const string	cEqBandCenterFrequencyProperty	= "bandCenterFreq";
		private const string	cEqBandGainProperty				= "bandGain";

		[ConfigurationPropertyAttribute( cEqBandIdProperty, IsRequired = true, IsKey = true, IsDefaultCollection = false )]
		public long BandId {
			get { return ((long)( base[cEqBandIdProperty] ) ); }
			set { base[cEqBandIdProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cEqBandCenterFrequencyProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "0.0" )]
		public float CenterFrequency {
			get { return ((float)( base[cEqBandCenterFrequencyProperty] ) ); }
			set { base[cEqBandCenterFrequencyProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cEqBandGainProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "0.0" )]
		public float Gain {
			get { return ((float)( base[cEqBandGainProperty] ) ); }
			set { base[cEqBandGainProperty] = value; }
		}

		public override string ToString() {
			return( BandId.ToString());
		}
	}
}
