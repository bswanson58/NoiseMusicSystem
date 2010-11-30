using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Practices.Unity;
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

	public class LicenseManager : ILicenseManager {
		private readonly ILog							mLog;
		private readonly Dictionary<string, LicenseKey>	mKeys;

		public LicenseManager( IUnityContainer container ) {
			mLog = container.Resolve<ILog>();

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
				mLog.LogException( "Exception - LicenseManager:Initialize ", ex );
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
