using System;
using System.Diagnostics;
using System.Reflection;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Logging;

namespace Noise.AppSupport.Logging {
	public class NoiseLogger : INoiseLog {
		private readonly IPlatformLog	mPlatformLog;
		private readonly IPreferences	mPreferences;
		private LoggingPreferences		mLoggingPreferences;

		public NoiseLogger( IPlatformLog log,  IPreferences preferences ) {
			mPlatformLog = log;
			mPreferences = preferences;
		}

		private LoggingPreferences Preferences {
			get {
				if( mLoggingPreferences == null ) {
					mLoggingPreferences = mPreferences.Load<LoggingPreferences>();
				}

				return( mLoggingPreferences );
				
			}
		}

		public void LogException( string message, Exception exception ) {
			if( Preferences.LogExceptions ) {
				var frame = new StackFrame( 1 );
				var method = frame.GetMethod();
				var type = method.DeclaringType;
				var name = type != null ? string.Format( "{0}:{1}", type.Name, method.Name ) : method.Name;

				LogException( message, exception, name );
			}
		}

		public void LogException( string message, Exception exception, string methodName ) {
			if( Preferences.LogExceptions ) {
				mPlatformLog.LogException( string.Format( "{0} - {1}", methodName, message ), exception );

				if( exception is ReflectionTypeLoadException ) {
					var tle = exception as ReflectionTypeLoadException;

					foreach( var exc in tle.LoaderExceptions ) {
						mPlatformLog.LogException( "ReflectionTypeLoadException:", exc );
					}
				}

				if( exception.InnerException != null ) {
					LogException( string.Format( ">>>Inner Exception of '{0}'", message ), exception.InnerException, methodName );
				}
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

		public void LogMessage( string message, string methodName ) {
			if( Preferences.LogMessages ) {
				mPlatformLog.LogMessage( string.Format( "{0} - {1}", methodName, message ));
			}
		}
	}
}
