using System;
using System.Diagnostics;
using System.Reflection;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Logging;

namespace Noise.Infrastructure {
	public class NoiseLogger : INoiseLog {
		private static IPlatformLog			mPlatformLog;
		private static INoiseLog			mCurrent;
		private static LoggingPreferences	mPreferences;

		public static void SetPlatformLogger( IPlatformLog platformLog ) {
			mPlatformLog = platformLog;
		}

		[Obsolete("NoiseLogger.Current is obsolete - use and instance of INoiseLog")]
		public static INoiseLog Current {
			get { return( mCurrent ?? ( mCurrent = new NoiseLogger())); }

			set {
				mCurrent = value;
			}
		}

		public static LoggingPreferences Preferences {
			get {
				if( mPreferences == null ) {
					mPreferences = new LoggingPreferences();
				}

				return( mPreferences );
			}

			set { mPreferences = value; }
		}

		public void LogException( string message, Exception ex ) {
			if( Preferences.LogExceptions ) {
				var frame = new StackFrame( 1 );
				var method = frame.GetMethod();
				var type = method.DeclaringType;
				var name = type != null ? string.Format( "{0}:{1}", type.Name, method.Name ) : method.Name;

				LogException( message, ex, name );
			}
		}

		public void LogException( string message, Exception ex, string methodName ) {
			if( Preferences.LogExceptions ) {
				mPlatformLog.LogException( string.Format( "{0} - {1}", methodName, message ), ex );

				if( ex is ReflectionTypeLoadException ) {
					var tle = ex as ReflectionTypeLoadException;

					foreach( var exc in tle.LoaderExceptions ) {
						mPlatformLog.LogException( "ReflectionTypeLoadException:", exc );
					}
				}

				if( ex.InnerException != null ) {
					LogException( string.Format( ">>>Inner Exception of '{0}'", message ), ex.InnerException, methodName );
				}
			}
		}

		public void LogMessage( string message, string methodName ) {
			if( Preferences.LogMessages ) {
				mPlatformLog.LogMessage( string.Format( "{0} - {1}", methodName, message ));
			}
		}

		public void LogMessage( string message ) {
			if( Preferences.LogMessages ) {
				var frame = new StackFrame( 1 );
				var method = frame.GetMethod();
				var type = method.DeclaringType;
				var name = type != null ? string.Format( "{0}:{1}", type.Name, method.Name ) : method.Name;

				mPlatformLog.LogMessage( string.Format( "{0} - {1}", name, message ));
			}
		}
	}
}
