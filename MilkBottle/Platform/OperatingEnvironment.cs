using System;
using System.IO;
using MilkBottle.Infrastructure.Interfaces;

namespace MilkBottle.Platform {
    class OperatingEnvironment : IEnvironment {
        private readonly IApplicationConstants  mApplicationConstants;

        public OperatingEnvironment( IApplicationConstants applicationConstants ) {
            mApplicationConstants = applicationConstants;
        }

        public string ApplicationName() {
            return mApplicationConstants.ApplicationName;
        }

        public string EnvironmentName() {
            return Environment.MachineName;
        }

        public string ApplicationDirectory() {
            var retValue = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.CommonApplicationData ),
                mApplicationConstants.CompanyName,
                mApplicationConstants.ApplicationName );

            if(!Directory.Exists( retValue )) {
                Directory.CreateDirectory( retValue );
            }

            return( retValue );
        }

        public string DatabaseDirectory() {
            var retValue = Path.Combine( ApplicationDirectory(), mApplicationConstants.DatabaseDirectory );

            if(!Directory.Exists( retValue )) {
                Directory.CreateDirectory( retValue );
            }

            return( retValue );
        }

        public string LogFileDirectory() {
            var retValue = Path.Combine( ApplicationDirectory(), mApplicationConstants.LogFileDirectory );

            if(!Directory.Exists( retValue )) {
                Directory.CreateDirectory( retValue );
            }

            return( retValue );
        }

        public string PreferencesDirectory() {
            var retValue = Path.Combine( ApplicationDirectory(), mApplicationConstants.ConfigurationDirectory );

            if(!Directory.Exists( retValue )) {
                Directory.CreateDirectory( retValue );
            }

            return( retValue );
        }

        public string MilkLibraryFolder() {
            var retValue = Path.Combine( ApplicationDirectory(), mApplicationConstants.MilkLibraryFolder );

            if(!Directory.Exists( retValue )) {
                Directory.CreateDirectory( retValue );
            }

            return( retValue );
        }

        public string MilkTextureFolder() {
            var retValue = Path.Combine( ApplicationDirectory(), mApplicationConstants.MilkTextureFolder );

            if(!Directory.Exists( retValue )) {
                Directory.CreateDirectory( retValue );
            }

            return( retValue );
        }
    }
}
