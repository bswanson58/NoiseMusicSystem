using System;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Logging;

namespace Noise.Core.Logging {
	internal class LogPlayStrategy : BaseLogger, ILogPlayStrategy {
		private readonly LoggingPreferences		mPreferences;

		private const string	cModuleName = "Playback";
		private const string	cPhaseName = "Strategy";

		public LogPlayStrategy( IPreferences preferences, IPlatformLog logger ) :
		base( logger ) {
			mPreferences = preferences.Load<LoggingPreferences>();
		}

		private void LogStrategyMessage( string format, params object[] parameters ) {
			LogMessage( cModuleName, cPhaseName, format, parameters );
		}

		public void LogTrackQueued( ePlayExhaustedStrategy strategy, DbTrack track ) {
			LogOnCondition( mPreferences.PlayQueueStrategy, () => LogStrategyMessage( "Strategy {0} has queued {1}", strategy, track ));
		}

		public void LogException( string message, Exception exception, string callerName = "" ) {
			LogOnCondition( mPreferences.LogExceptions, () => LogException( cModuleName, cPhaseName, exception, callerName, "{0}", message ));
		}
	}
}
