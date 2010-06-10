using System;
using System.ComponentModel.Composition;
using NLog;

namespace Noise.Infrastructure {
	[Export( typeof(ILog))]
	public class Log : ILog {
		private static readonly Logger	mLogger = LogManager.GetLogger( "NoiseLogger" );

		public void LogException( Exception ex ) {
			LogException( "", ex );
		}

		public void LogException( string message, Exception ex ) {
			mLogger.ErrorException( message, ex );
		}

		public void LogMessage( string message ) {
			mLogger.Info( message );
		}

		public void LogInfo( string message ) {
			mLogger.Debug( message );
		}
	}
}
