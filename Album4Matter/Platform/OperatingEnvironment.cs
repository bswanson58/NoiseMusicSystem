using System;
using System.IO;
using Album4Matter.Interfaces;
using Album4Matter.Properties;

namespace Album4Matter.Platform {
    class OperatingEnvironment : IEnvironment {
        private readonly string		mApplicationName;

        public OperatingEnvironment() {
            mApplicationName = ApplicationConstants.ApplicationName;
        }

        public string ApplicationName() {
            return( mApplicationName );
        }

        public string ApplicationDirectory() {
            var retValue = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.CommonApplicationData ),
                ApplicationConstants.CompanyName,
                ApplicationConstants.ApplicationName );

            if(!Directory.Exists( retValue )) {
                Directory.CreateDirectory( retValue );
            }

            return( retValue );
        }

        public string LogFileDirectory() {
            var retValue = Path.Combine( ApplicationDirectory(), ApplicationConstants.LogFileDirectory );

            if(!Directory.Exists( retValue )) {
                Directory.CreateDirectory( retValue );
            }

            return( retValue );
        }

        public string PreferencesDirectory() {
            var retValue = Path.Combine( ApplicationDirectory(), ApplicationConstants.ConfigurationDirectory );

            if(!Directory.Exists( retValue )) {
                Directory.CreateDirectory( retValue );
            }

            return( retValue );
        }
    }
}
