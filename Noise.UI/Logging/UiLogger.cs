using System;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Logging;

namespace Noise.UI.Logging {
	internal class UiLogger : BaseLogger, IUiLog {
		private readonly LoggingPreferences		mPreferences;

		private const string	cModuleName = "UserInterface";

		public UiLogger( IPreferences preferences, IPlatformLog logger ) :
		base( logger ) {
			mPreferences = preferences.Load<LoggingPreferences>();
		}

		private void LogUiMessage( string callerName, string format, params object[] parameters ) {
			LogMessage( cModuleName, callerName, format, parameters );
		}

		public void LogMessage( string message, string callerName = "" ) {
			LogOnCondition( mPreferences.UserInterface, () => LogUiMessage( callerName, "{0}", message ));
		}

		public void LogException( string message, Exception exception, string callerName = "" ) {
			LogOnCondition( mPreferences.LogExceptions, () => LogException( cModuleName, callerName, exception, callerName, "{0}", message ));
		}
	}
}
