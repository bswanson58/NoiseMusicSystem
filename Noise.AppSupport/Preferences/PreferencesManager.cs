using System;
using System.IO;
using Noise.Infrastructure.Interfaces;

namespace Noise.AppSupport.Preferences {
	public class PreferencesManager : IPreferences {
		private readonly INoiseEnvironment	mEnvironment;
		private readonly IFileWriter		mWriter;
		private readonly Lazy<INoiseLog>	mLog;

		public PreferencesManager( INoiseEnvironment noiseEnvironment, IFileWriter writer, Lazy<INoiseLog> log   ) {
			mEnvironment = noiseEnvironment;
			mWriter = writer;
			mLog = log;
		}

		protected virtual T CreateDefault<T>() {
			return( Activator.CreateInstance<T>());
		}

		public T Load<T>() where T : new() {
			var retValue = default( T );

			try {
				mWriter.Read<T>( Path.Combine( mEnvironment.PreferencesDirectory(), typeof( T ).Name ));
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
					mWriter.Write( Path.Combine( mEnvironment.PreferencesDirectory(), typeof( T ).Name ), preferences );
				}
				catch( Exception ex ) {
					mLog.Value.LogException( string.Format( "Saving Preferences failed for '{0}'", typeof( T ).Name ), ex );
				}
			}
		}
	}
}
