using System;
using System.IO;
using Newtonsoft.Json;
using Noise.Infrastructure.Interfaces;

namespace Noise.AppSupport.Preferences {
	public class PreferencesManager : IPreferences {
		private readonly INoiseEnvironment	mEnvironment;
		private readonly Lazy<INoiseLog>	mLog;

		public PreferencesManager( INoiseEnvironment noiseEnvironment, Lazy<INoiseLog> log   ) {
			mEnvironment = noiseEnvironment;
			mLog = log;
		}

		protected virtual T CreateDefault<T>() {
			return( Activator.CreateInstance<T>());
		}

		public T Load<T>() where T : new() {
			var retValue = default( T );
			var preferencesFile = Path.Combine( mEnvironment.PreferencesDirectory(), typeof( T ).Name );

			try {
				if( File.Exists( preferencesFile )) {
					using( var file = File.OpenText( preferencesFile )) {
						retValue = JsonConvert.DeserializeObject<T>( file.ReadToEnd());
					}
				}
			}
			catch( Exception ex ) {
				mLog.Value.LogException( string.Format( "Loading Preferences failed for '{0}'", typeof( T ).Name ), ex );
			}

			if( Equals( retValue, default( T ))) {
				retValue = CreateDefault<T>();
			}

			return( retValue );
		}

		public void Save<T>( T preferences ) {
			if(!Equals( preferences, null )) {
				try {
					var preferencesFile = Path.Combine( mEnvironment.PreferencesDirectory(), typeof( T ).Name );
					var	json = JsonConvert.SerializeObject( preferences, Formatting.Indented );

					using( var file = File.CreateText( preferencesFile )) {
						file.Write( json );
						file.Close();
					}
				}
				catch( Exception ex ) {
					mLog.Value.LogException( string.Format( "Saving Preferences failed for '{0}'", typeof( T ).Name ), ex );
				}
			}
		}
	}
}
