using System;
using System.IO;
using MilkBottle.Interfaces;

namespace MilkBottle.Platform {
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
            return Load<T>( Path.Combine( mEnvironment.PreferencesDirectory(), typeof( T ).Name ));
        }

        public T Load<T>( string path ) where T : new() {
            var retValue = default( T );

            try {
                retValue = mWriter.Read<T>( path );
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
            Save( preferences, Path.Combine( mEnvironment.PreferencesDirectory(), typeof( T ).Name ));
        }

        public void Save<T>( T preferences, string path ) {
            if(!Equals( preferences, null )) {
                try {
                    mWriter.Write( path, preferences );
                }
                catch( Exception ex ) {
                    mLog.Value.LogException( $"Saving Preferences failed for '{typeof( T ).Name}'", ex );
                }
            }
        }
    }
}
