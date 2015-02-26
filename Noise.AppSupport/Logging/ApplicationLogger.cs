using System;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Logging;

namespace Noise.AppSupport.Logging {
	internal class ApplicationLogger : BaseLogger, IApplicationLog {
		private readonly LoggingPreferences		mPreferences;
		private readonly INoiseEnvironment		mEnvironment;

		private const string	cModuleName = "Application";
		private const string	cName = "Shell";

		public ApplicationLogger( IPlatformLog log,  LoggingPreferences preferences, INoiseEnvironment environment ) :
			base( log ) {
			mPreferences = preferences;
			mEnvironment = environment;
		}

		private void LogApplicationMessage( string format, params object[] parameters ) {
			LogMessage( cModuleName, cName, format, parameters );
		}

		public void ApplicationStarting() {
			LogOnCondition( mPreferences.LogMessages || mPreferences.BasicActivity, () => LogApplicationMessage( string.Format( "+++++ {0} application starting +++++", mEnvironment.ApplicationName())));
		}

		public void ApplicationExiting() {
			LogOnCondition( mPreferences.LogMessages || mPreferences.BasicActivity, () => LogApplicationMessage( string.Format( "===== {0} application exiting =====", mEnvironment.ApplicationName())));
		}

		public void LogException( string message, Exception exception, string callerName = "" ) {
			LogOnCondition( mPreferences.LogExceptions, () => LogException( cModuleName, cName, exception, callerName, "{0}", message ));
		}
	}
}
