using System;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Logging;

namespace Noise.RavenDatabase.Logging {
	internal class RavenLogger : BaseLogger, ILogRaven {
		private readonly LoggingPreferences		mPreferences;

		private const string	cModuleName = "Raven";
		private const string	cPhaseName = "Database";

		public RavenLogger( IPreferences preferences, IPlatformLog logger ) :
		base( logger ) {
			mPreferences = preferences.Load<LoggingPreferences>();
		}

		private void LogDatabaseMessage( string format, params object[] parameters ) {
			LogMessage( cModuleName, cPhaseName, format, parameters );
		}

		public void CreatedDatabase( LibraryConfiguration library ) {
			LogOnCondition( mPreferences.DatabaseActivity, () => LogDatabaseMessage( "Created {0}", library ));
		}

		public void OpenedDatabase( LibraryConfiguration library ) {
			LogOnCondition( mPreferences.DatabaseActivity, () => LogDatabaseMessage( "Opened {0}", library ));
		}

		public void ClosedDatabase() {
			LogOnCondition( mPreferences.DatabaseActivity, () => LogDatabaseMessage( "Closed Library" ));
		}

		public void AddingItem( DbBase item ) {
			LogOnCondition( mPreferences.DatabaseActivity, () => LogDatabaseMessage( "Adding {0}", item ));
		}

		public void AddingExistingItem( DbBase item ) {
			LogOnCondition( mPreferences.DatabaseActivity, () => LogDatabaseMessage( "Attempting to add existing item {0}", item ));
		}

		public void RemoveItem( DbBase item ) {
			LogOnCondition( mPreferences.DatabaseActivity, () => LogDatabaseMessage( "Removing {0}", item ));
		}

		public void LogException( string message, Exception exception, string callerName = "" ) {
			LogOnCondition( mPreferences.LogExceptions, () => LogException( cModuleName, cPhaseName, exception, callerName, "{0}", message ));
		}
	}
}
