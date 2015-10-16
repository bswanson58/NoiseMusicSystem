using System;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;

namespace Noise.AppSupport.Preferences {
	public class HeadlessPreferences : PreferencesManager {
		public HeadlessPreferences( INoiseEnvironment noiseEnvironment, IFileWriter writer, Lazy<INoiseLog> log ) :
			base( noiseEnvironment, writer, log ) { }

		protected override T CreateDefault<T>() {
			var retValue = base.CreateDefault<T>();

			if( retValue is NoiseCorePreferences ) {
				var preferences = retValue as NoiseCorePreferences;

				preferences.EnableRemoteAccess = true;
			}

			return( retValue );
		}
	}
}
