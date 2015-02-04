using System;
using System.IO;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.AppSupport {
	public class NoiseEnvironment : INoiseEnvironment {
		private readonly string		mApplicationName;

		public NoiseEnvironment( string applicationName ) {
			mApplicationName = applicationName;
		}

		public string ApplicationDirectory() {
			var retValue = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.CommonApplicationData ),
										 Constants.CompanyName,
										 Constants.ApplicationName );

			if(!Directory.Exists( retValue )) {
				Directory.CreateDirectory( retValue );
			}

			return( retValue );
		}

		public string LibraryDirectory() {
			var retValue = Path.Combine( ApplicationDirectory(), Constants.LibraryConfigurationDirectory );

			if(!Directory.Exists( retValue )) {
				Directory.CreateDirectory( retValue );
			}

			return( retValue );
		}

		public string LogFileDirectory() {
			var retValue = Path.Combine( ApplicationDirectory(), Constants.LogFileDirectory, mApplicationName );

			if(!Directory.Exists( retValue )) {
				Directory.CreateDirectory( retValue );
			}

			return( retValue );
		}

		public string BackupDirectory() {
			var retValue = Path.Combine( ApplicationDirectory(), Constants.LibraryBackupDirectory );

			if(!Directory.Exists( retValue )) {
				Directory.CreateDirectory( retValue );
			}

			return( retValue );
		}

		public string ConfigurationDirectory() {
			var retValue = Path.Combine( ApplicationDirectory(), Constants.ConfigurationDirectory );

			if(!Directory.Exists( retValue )) {
				Directory.CreateDirectory( retValue );
			}

			return( retValue );
		}

		public string PreferencesDirectory() {
			var retValue = Path.Combine( ApplicationDirectory(), Constants.ConfigurationDirectory, mApplicationName );

			if(!Directory.Exists( retValue )) {
				Directory.CreateDirectory( retValue );
			}

			return( retValue );
		}
	}
}
