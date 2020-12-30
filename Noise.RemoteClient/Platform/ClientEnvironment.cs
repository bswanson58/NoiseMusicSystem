using System;
using System.IO;
using Noise.RemoteClient.Interfaces;

namespace Noise.RemoteClient.Platform {
    class ClientEnvironment : IClientEnvironment {
        private const string    cCompanyName        = "Secret Squirrel Software";
        private const string    cApplicationName    = "Noise Remote";
        private const string    cLogDirectory       = "Logs";

        public string ApplicationDirectory {
            get {
                var retValue = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ), cCompanyName, cApplicationName );

                try {
                    if(!Directory.Exists( retValue )) {
                        Directory.CreateDirectory( retValue );
                    }
                }
                catch( Exception ) {
                    // the platform log requires this environment class...
                }

                return retValue;
            }
        }

        public string LogDirectory {
            get {
                var retValue = Path.Combine( ApplicationDirectory, cLogDirectory );
                try {
                    if(!Directory.Exists( retValue )) {
                        Directory.CreateDirectory( retValue );
                    }
                }
                catch( Exception ) {
                    // the platform log requires this environment class...
                }

                return retValue;
            }
        } 
    }
}
