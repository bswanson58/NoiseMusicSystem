using System;
using System.Diagnostics;
using System.Reflection;
using Noise.Infrastructure.Interfaces;

namespace Noise.Infrastructure {
	public class NoiseLogger : INoiseLog {
		private static IPlatformLog			mPlatformLog;
		private static INoiseLog			mCurrent;

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

		public void LogException( string message, Exception ex ) {
			var frame = new StackFrame( 1 );
			var method = frame.GetMethod();
			var type = method.DeclaringType;
			var name = type != null ? string.Format( "{0}:{1}", type.Name, method.Name ) : method.Name;

			mPlatformLog.LogException( string.Format( "{0} - {1}", name, message ), ex );

			if( ex is ReflectionTypeLoadException ) {
				var tle = ex as ReflectionTypeLoadException;

				foreach( var exc in tle.LoaderExceptions ) {
					mPlatformLog.LogException( "ReflectionTypeLoadException:", exc );
				}
			}

			if( ex.InnerException != null ) {
				LogException( string.Format( ">>>Inner Exception of '{0}'", message ), ex.InnerException );
			}
		}

		public void LogMessage( string format, params object[] parameters ) {
			var frame = new StackFrame( 1 );
			var method = frame.GetMethod();
			var type = method.DeclaringType;
			var name = type != null ? string.Format( "{0}:{1}", type.Name, method.Name ) : method.Name;

			mPlatformLog.LogMessage( string.Format( "{0} - {1}", name,  string.Format( format, parameters )));
		}

		public void LogMessage( string message ) {
			var frame = new StackFrame( 1 );
			var method = frame.GetMethod();
			var type = method.DeclaringType;
			var name = type != null ? string.Format( "{0}:{1}", type.Name, method.Name ) : method.Name;

			mPlatformLog.LogMessage( string.Format( "{0} - {1}", name, message ));
		}

		public void LogInfo( string message ) {
			var frame = new StackFrame( 1 );
			var method = frame.GetMethod();
			var type = method.DeclaringType;
			var name = type != null ? string.Format( "{0}:{1}", type.Name, method.Name ) : method.Name;

			mPlatformLog.LogMessage( string.Format( "{0} - {1}", name, message ));
		}
	}
}
