using System;
using Noise.Core.Database;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Logging;

namespace Noise.Core.Logging {
	internal class LogLibraryBuilding : BaseLogger, ILogLibraryBuilding {
		private readonly LoggingPreferences		mPreferences;

		private const string	cModuleName = "Library Building";
		private const string	cPhaseName = "Builder";

		public LogLibraryBuilding( IPreferences preferences, IPlatformLog logger ) :
		base( logger ) {
			mPreferences = preferences.Load<LoggingPreferences>();
		}

		private void LogBuildingMessage( string format, params object[] parameters ) {
			LogMessage( cModuleName, cPhaseName, format, parameters );
		}

		public void BuildingStarted() {
			LogOnCondition( mPreferences.LogAnyBuilding, () => LogBuildingMessage( "Starting library building" ));
		}

		public void BuildingCompleted( DatabaseChangeSummary summary ) {
			LogOnCondition( mPreferences.LogAnyBuilding, () => LogBuildingMessage( "Completed library building: {0}", summary ));
		}

		public void DatabaseStatistics( DatabaseStatistics statistics ) {
			LogOnCondition( mPreferences.LogAnyBuilding, () => LogBuildingMessage( "{0}", statistics ));
		}

		public void LogException( string message, Exception exception, string callerName = "" ) {
			LogOnCondition( mPreferences.LogExceptions, () => LogException( cModuleName, cPhaseName, exception, callerName, "{0}", message ));
		}
	}
}
