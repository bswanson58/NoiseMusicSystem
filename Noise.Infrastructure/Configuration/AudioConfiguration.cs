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

		public override bool IsReadOnly() {
			return( false );
		}

		[ConfigurationPropertyAttribute( cDefaultEqualizerProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "0" )]
		public long DefaultEqualizer {
			get { return ((long)( base[cDefaultEqualizerProperty] ) ); }
			set { base[cDefaultEqualizerProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cPreampGainProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "1.0" )]
		public float PreampGain {
			get { return ((float)( base[cPreampGainProperty] ) ); }
			set { base[cPreampGainProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cEnableReplayGainProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "true" )]
		public bool ReplayGainEnabled {
			get { return ((bool)( base[cEnableReplayGainProperty] ) ); }
			set { base[cEnableReplayGainProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cEqualizationEnabledProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "false" )]
		public bool EqEnabled {
			get { return ((bool)( base[cEqualizationEnabledProperty] ) ); }
			set { base[cEqualizationEnabledProperty] = value; }
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
