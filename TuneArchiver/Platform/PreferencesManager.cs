using System;
using System.IO;
using TuneArchiver.Interfaces;

namespace TuneArchiver.Platform {
    public class PreferencesManager : IPreferences {
        private readonly IEnvironment	    mEnvironment;
        private readonly IFileWriter		mWriter;
        private readonly Lazy<IPlatformLog>	mLog;

        public PreferencesManager( IEnvironment noiseEnvironment, IFileWriter writer, Lazy<IPlatformLog> log   ) {
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
                retValue = mWriter.Read<T>( Path.Combine( mEnvironment.PreferencesDirectory(), typeof( T ).Name ));
            }
            catch( Exception ex ) {
                mLog.Value.LogException( $"Loading Preferences failed for '{typeof( T ).Name}'", ex );
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
                    mLog.Value.LogException( $"Saving Preferences failed for '{typeof( T ).Name}'", ex );
                }
            }
        }
    }
}
