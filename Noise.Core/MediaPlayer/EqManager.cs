using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.MediaPlayer {
	public class EqManager : IEqManager {
		private readonly IUnityContainer			mContainer;
		private readonly ILog						mLog;
		private readonly List<ParametricEqualizer>	mEqPresets;
		private ParametricEqualizer					mCurrentEq;

		public EqManager( IUnityContainer container ) {
			mContainer = container;
			mLog = mContainer.Resolve<ILog>();

			mEqPresets = new List<ParametricEqualizer>();
		}

		public bool Initialize( string fileName ) {
			var retValue = LoadPresets( fileName );

			retValue &= LoadUserEq();

			if(( mCurrentEq == null ) &&
			   ( mEqPresets.Count > 0 )) {
				mCurrentEq = mEqPresets[0];
			}

			return( retValue );
		}

		private bool LoadPresets( string fileName ) {
			var retValue = false;

			try {
				var presetDoc = XDocument.Load( fileName );
				var presets = from element in presetDoc.Descendants( "eq" ) select element;

				foreach( var presetElement in presets ) {
					var preset = new ParametricEqualizer((long)presetElement.Attribute( "id" ), true, (string)presetElement.Attribute( "name" ),
														 (string)presetElement.Element( "description" ), (float)presetElement.Attribute( "bandwidth" ));
					var bandElements = from element in presetElement.Descendants( "band" ) select element;

					foreach( var band in bandElements ) {
						preset.AddBand((float)band.Attribute( "centerFrequency" ), (float)band.Attribute( "gain" ));
					}

					mEqPresets.Add( preset );
				}

				retValue = true;
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - EqPresets:LoadPresets ", ex );
			}

			return( retValue );
		}

		private bool LoadUserEq() {
			var retValue = false;
			var	systemConfig = mContainer.Resolve<ISystemConfiguration>();
			var audioCongfiguration = systemConfig.RetrieveConfiguration<AudioConfiguration>( AudioConfiguration.SectionName );
			if( audioCongfiguration != null ) {
				var equalizerList = ( from ParametricEqConfiguration eqConfig in audioCongfiguration.ParametricEqualizers
				                      select eqConfig.AsParametericEqualizer()).ToList();
				if( equalizerList.Count > 0 ) {
					mEqPresets.AddRange( equalizerList );
				}
				else {
					var eq = new ParametricEqualizer { Bandwidth = 18f, Name = "User 1" };
					eq.AddBands( new [] { new ParametricBand( 120.0f ), new ParametricBand( 250.0f ),
											new ParametricBand( 500.0f ), new ParametricBand( 1000.0f ),
											new ParametricBand( 2000.0f ), new ParametricBand( 4000.0f ),
											new ParametricBand( 8000.0f ), new ParametricBand( 12000.0f ) });
					mEqPresets.Add( eq );
					mCurrentEq = eq;
				}

				if( mCurrentEq == null ) {
					mCurrentEq = ( from ParametricEqualizer eq in mEqPresets where eq.EqualizerId == audioCongfiguration.DefaultEqualizer select eq ).FirstOrDefault();
				}

				retValue = true;
			}

			return( retValue );
		}

		public bool SaveEq( ParametricEqualizer eq, bool eqEnabled ) {
			var retValue = false;

			if(( eq != null ) &&
			   (!eq.IsPreset )) {
				var	systemConfig = mContainer.Resolve<ISystemConfiguration>();
				var audioCongfiguration = systemConfig.RetrieveConfiguration<AudioConfiguration>( AudioConfiguration.SectionName );
				if( audioCongfiguration != null ) {
					audioCongfiguration.UpdateEq( eq );
					audioCongfiguration.EqEnabled = eqEnabled;

					systemConfig.Save( audioCongfiguration );

					retValue = true;
				}
			}

			return( retValue );
		}

		public IEnumerable<ParametricEqualizer> EqPresets {
			get{ return( mEqPresets ); }
		}

		public ParametricEqualizer CurrentEq {
			get{ return( mCurrentEq ); }
			set{
				if(( value != null ) &&
				   ( mCurrentEq.EqualizerId != value.EqualizerId )) {
					mCurrentEq = value;

					var	systemConfig = mContainer.Resolve<ISystemConfiguration>();
					var audioCongfiguration = systemConfig.RetrieveConfiguration<AudioConfiguration>( AudioConfiguration.SectionName );
					if( audioCongfiguration != null ) {
						audioCongfiguration.DefaultEqualizer = mCurrentEq.EqualizerId;
					}
				}
			}
		}
	}
}
