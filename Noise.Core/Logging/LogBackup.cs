using System;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Logging;

namespace Noise.Core.Logging {
    class LogBackup : BaseLogger, ILogBackup {
        private const string	cModuleName = "Library Backup";
        private const string	cPhaseName = "Backup Pressure";

        private readonly LoggingPreferences		mPreferences;

        public LogBackup( IPreferences preferences, IPlatformLog logger ) :
            base( logger ) {
            mPreferences = preferences.Load<LoggingPreferences>();
        }

        private void LogBackupMessage( string format, params object[] parameters ) {
            LogMessage( cModuleName, cPhaseName, format, parameters );
        }

        public void LogBackupPressure( string source, uint currentPressure, int currentThreshold ) {
            LogOnCondition( mPreferences.BackupTasks, () => LogBackupMessage( "Backup Pressure from {0}, pressure is: {1}/{2}", source, currentPressure, currentThreshold ));
        }

        public void LogBackupThresholdExceeded( string libraryName, int currentThreshold ) {
            LogOnCondition( mPreferences.BackupTasks, () => LogBackupMessage( "Backup pressure exceeded for library: {0}. Threshold is: {1}", libraryName, currentThreshold ));
        }

        public void LogLibraryBackup( string libraryName ) {
            LogOnCondition( mPreferences.BackupTasks, () => LogBackupMessage( "Backup completed for library: {0}", libraryName ));
        }

        public void LogBackupException( Exception ex ) {
            LogException( cModuleName, cPhaseName, ex, "LogBackup", "Backup failed." );
        }
    }
}
