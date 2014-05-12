using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Noise.Infrastructure.Interfaces;

namespace Noise.Infrastructure.Support {
	public class LicenseKey {
		public string	Name { get; private set; }
		public string	Key { get; private set; }

		public LicenseKey( string name, string key ) {
			Name = name;
			Key = key;
		}
	}

	public class NoiseLicenseManager : ILicenseManager {
		private readonly INoiseEnvironment	mNoiseEnvironment;

		private readonly Dictionary<string, LicenseKey>	mKeys;

		public NoiseLicenseManager( INoiseEnvironment noiseEnvironment ) {
			mNoiseEnvironment = noiseEnvironment;

			mKeys = new Dictionary<string, LicenseKey>();
		}

		private void Initialize() {
			var licenseFile = Path.Combine( mNoiseEnvironment.ConfigurationDirectory(), Constants.LicenseKeyFile + ".debug" );

			if(!File.Exists( licenseFile )) {
				licenseFile = Path.Combine( mNoiseEnvironment.ConfigurationDirectory(), Constants.LicenseKeyFile );
			}

			try {
				var presetDoc = XDocument.Load( licenseFile );
				var licenseKeys = from element in presetDoc.Descendants( "licenseKey" ) select element;

				foreach( var keyElement in licenseKeys ) {
					var key = (string)keyElement.Attribute( "id" );
					var license = new LicenseKey((string)keyElement.Element( "name" ), (string)keyElement.Element( "key" ));

					mKeys.Add( key, license );
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - LicenseManager:Initialize ", ex );
			}
		}

		public LicenseKey RetrieveKey( string keyId ) {
			LicenseKey	retValue = null;

			if(!mKeys.Keys.Any()) {
				Initialize();
			}

			if( mKeys.ContainsKey( keyId )) {
				retValue = mKeys[keyId];
			}
			return( retValue );
		}
	}
}
