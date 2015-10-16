using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Noise.AudioSupport.Logging;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.AudioSupport.Player {
	public class EqManager : IEqManager {
		private readonly IPreferences				mPreferences;
		private readonly ILogAudioPlay				mLog;
		private readonly List<ParametricEqualizer>	mEqPresets;
		private ParametricEqualizer					mCurrentEq;
		private bool								mEqEnabled;

		public EqManager( IPreferences preferences, ILogAudioPlay log ) {
			mPreferences = preferences;
			mLog = log;

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
				mLog.LogException( "Loading EQ presets", ex );
			}

			return( retValue );
		}

		private bool LoadUserEq() {
			var retValue = false;
			var audioCongfiguration = mPreferences.Load<AudioPreferences>();
			if( audioCongfiguration != null ) {
				var equalizerList = ( from ParametricEqConfiguration eqConfig in audioCongfiguration.ParametricEqualizers
				                      select eqConfig.AsParametericEqualizer()).ToList();
				if( equalizerList.Count > 0 ) {
					mEqPresets.AddRange( equalizerList );
				}
				else {
					var eq = new ParametricEqualizer { Bandwidth = 18f, Name = "User 1" };
					eq.AddBands( new [] { new ParametricBand( 30.0f ), new ParametricBand( 60.0f ),
										  new ParametricBand( 120.0f ), new ParametricBand( 250.0f ),
										  new ParametricBand( 500.0f ), new ParametricBand( 1000.0f ),
										  new ParametricBand( 2000.0f ), new ParametricBand( 4000.0f ),
										  new ParametricBand( 8000.0f ), new ParametricBand( 16000.0f ) });
					mEqPresets.Add( eq );
					mCurrentEq = eq;
				}

				if( mCurrentEq == null ) {
					mCurrentEq = ( from ParametricEqualizer eq in mEqPresets where eq.EqualizerId == audioCongfiguration.DefaultEqualizer select eq ).FirstOrDefault();
				}

				mEqEnabled = audioCongfiguration.EqEnabled;

				retValue = true;
			}

			return( retValue );
		}

		public bool SaveEq( ParametricEqualizer eq, bool eqEnabled ) {
			var retValue = false;

			if(( eq != null ) &&
			   (!eq.IsPreset )) {
				var audioCongfiguration = mPreferences.Load<AudioPreferences>();
				if( audioCongfiguration != null ) {
					audioCongfiguration.UpdateEq( eq );
					audioCongfiguration.EqEnabled = eqEnabled;
					audioCongfiguration.DefaultEqualizer = mCurrentEq != null ? mCurrentEq.EqualizerId : 0;

					mPreferences.Save( audioCongfiguration );

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

					var audioCongfiguration = mPreferences.Load<AudioPreferences>();
					if( audioCongfiguration != null ) {
						audioCongfiguration.DefaultEqualizer = mCurrentEq.EqualizerId;

						mPreferences.Save( audioCongfiguration );
					}
				}
			}
		}

		public bool EqEnabled {
			get{ return( mEqEnabled ); }
			set {
				mEqEnabled = value;
				
				var preferences = mPreferences.Load<AudioPreferences>();

				preferences.EqEnabled = mEqEnabled;
				mPreferences.Save( preferences );
			}
		}
	}
}
