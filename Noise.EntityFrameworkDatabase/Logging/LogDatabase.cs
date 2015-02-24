using System;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Logging;

namespace Noise.EntityFrameworkDatabase.Logging {
	internal class LogDatabase : BaseLogger, ILogDatabase {
		private readonly LoggingPreferences		mPreferences;

		private const string	cModuleName = "EntityFramework";
		private const string	cPhaseName = "Database";

		public LogDatabase( IPreferences preferences, IPlatformLog logger ) :
		base( logger ) {
			mPreferences = preferences.Load<LoggingPreferences>();
		}

		private void LogDatabaseMessage( string format, params object[] parameters ) {
			LogMessage( cModuleName, cPhaseName, format, parameters );
		}

		public void LogException( string message, Exception exception, string callerName = "" ) {
			LogOnCondition( mPreferences.LogExceptions, () => LogException( cModuleName, cPhaseName, exception, callerName, "{0}", message ));
		}
	}
}
