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
		private static ILicenseManager			mCurrent;

		private readonly Dictionary<string, LicenseKey>	mKeys;

		public static ILicenseManager Current {
			get {
				if( mCurrent == null ) {
					mCurrent = new NoiseLicenseManager();
					if(!mCurrent.Initialize( Constants.LicenseKeyFile )) {
						NoiseLogger.Current.LogMessage( "LicenseManager could not be initialized." );
					}
				}

				return( mCurrent );
			}

			set {
				mCurrent = value;
			}
		}

		public NoiseLicenseManager() {
			mKeys = new Dictionary<string, LicenseKey>();
		}

		public bool Initialize( string licenseFile ) {
			var retValue = false;

			if( File.Exists( licenseFile + ".debug" )) {
				licenseFile += ".debug";
			}

			try {
				var presetDoc = XDocument.Load( licenseFile );
				var licenseKeys = from element in presetDoc.Descendants( "licenseKey" ) select element;

				foreach( var keyElement in licenseKeys ) {
					var key = (string)keyElement.Attribute( "id" );
					var license = new LicenseKey((string)keyElement.Element( "name" ), (string)keyElement.Element( "key" ));

					mKeys.Add( key, license );
				}

				retValue = true;
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - LicenseManager:Initialize ", ex );
			}

			return( retValue );
		}

		public LicenseKey RetrieveKey( string keyId ) {
			LicenseKey	retValue = null;

			if( mKeys.ContainsKey( keyId )) {
				retValue = mKeys[keyId];
			}
			return( retValue );
		}
	}
}
