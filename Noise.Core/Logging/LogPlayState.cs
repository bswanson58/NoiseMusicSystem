using System;
using Noise.Core.PlaySupport;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Logging;

namespace Noise.Core.Logging {
	internal class LogPlayState : BaseLogger, ILogPlayState {
		private readonly LoggingPreferences		mPreferences;

		private const string	cModuleName = "Playback";
		private const string	cPhaseName = "PlayState";

		public LogPlayState( IPreferences preferences, ILog logger ) :
		base( logger ) {
			mPreferences = preferences.Load<LoggingPreferences>();
		}

		private void LogPlayStateMessage( string format, params object[] parameters ) {
			LogMessage( cModuleName, cPhaseName, format, parameters );
		}

		public void PlayStateTrigger( eStateTriggers trigger ) {
			LogOnCondition( mPreferences.PlayState, () => LogPlayStateMessage( " Trigger: {0}", trigger ));
		}

		public void PlayStateSet( ePlayState state ) {
			LogOnCondition( mPreferences.PlayState, () => LogPlayStateMessage( "In State: {0}", state ));
		}

		public void PlaybackStatusChanged( ePlaybackStatus status ) {
			LogOnCondition( mPreferences.PlayState, () => LogPlayStateMessage( "  Status: {0}", status ));
		}

		public void LogPlayStateException( string message, Exception exception, string callerName = "" ) {
			LogOnCondition( mPreferences.LogExceptions, () => LogException( cModuleName, cPhaseName, exception, callerName, "{0}", message ));
		}
	}
}
