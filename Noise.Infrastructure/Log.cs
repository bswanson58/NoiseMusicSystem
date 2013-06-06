using System;
using System.Reflection;
using NLog;
using Noise.Infrastructure.Interfaces;

namespace Noise.Infrastructure {
	public class NoiseLogger : ILog {
		private static readonly ILog	mDefaultContext = new NoiseLogger();
		private static readonly Logger	mLogger = LogManager.GetLogger( "NoiseLogger" );
		private static ILog				mCurrent;

		public static ILog Current {
			get { return( mCurrent ?? ( mCurrent = mDefaultContext )); }

			set {
				mCurrent = value;
			}
		}

		public void LogException( Exception ex ) {
			LogException( "", ex );
		}

		public void LogException( string message, Exception ex ) {
			mLogger.ErrorException( message, ex );

			if( ex is ReflectionTypeLoadException ) {
				var tle = ex as ReflectionTypeLoadException;

				foreach( var exc in tle.LoaderExceptions ) {
					mLogger.ErrorException( "ReflectionTypeLoadException:", exc );
				}
			}
		}

		public void LogMessage( string format, params object[] parameters ) {
			LogMessage( string.Format( format, parameters ));
		}

		public void LogMessage( string message ) {
			mLogger.Info( message );
		}

		public void LogInfo( string format, params object[] parameters ) {
			LogInfo( string.Format( format, parameters ));
		}

		public void LogInfo( string message ) {
			mLogger.Debug( message );
		}
	}
}
