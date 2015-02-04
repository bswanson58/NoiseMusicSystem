using System;
using System.Diagnostics;
using System.Reflection;
using Noise.Infrastructure.Interfaces;

namespace Noise.AppSupport.Logging {
	public class NoiseLogger : INoiseLog {
		private readonly	IPlatformLog	mPlatformLog;

		public NoiseLogger( IPlatformLog log ) {
			mPlatformLog = log;
		}

		public void LogException( string message, Exception exception ) {
			var frame = new StackFrame( 1 );
			var method = frame.GetMethod();
			var type = method.DeclaringType;
			var name = type != null ? string.Format( "{0}:{1}", type.Name, method.Name ) : method.Name;

			mPlatformLog.LogException( string.Format( "{0} - {1}", name, message ), exception );

			if( exception is ReflectionTypeLoadException ) {
				var tle = exception as ReflectionTypeLoadException;

				foreach( var exc in tle.LoaderExceptions ) {
					mPlatformLog.LogException( "ReflectionTypeLoadException:", exc );
				}
			}
		}

		public void LogMessage( string message ) {
			var frame = new StackFrame( 1 );
			var method = frame.GetMethod();
			var type = method.DeclaringType;
			var name = type != null ? string.Format( "{0}:{1}", type.Name, method.Name ) : method.Name;

			mPlatformLog.LogMessage( string.Format( "{0} - {1}", name, message ));
		}
	}
}
