using System;
using System.IO;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.AppSupport {
	public class NoiseEnvironment : INoiseEnvironment {
		public string ApplicationDirectory() {
			var retValue = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.CommonApplicationData ),
										 Constants.CompanyName );

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

		public string BackupDirectory() {
			var retValue = Path.Combine( ApplicationDirectory(), Constants.LibraryBackupDirectory );

			if(!Directory.Exists( retValue )) {
				Directory.CreateDirectory( retValue );
			}

			return( retValue );
		}
	}
}
