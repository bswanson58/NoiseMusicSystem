using System;

namespace Noise.Core.Logging {
    interface ILogBackup {
        void    LogBackupPressure( string source, uint currentPressure, int currentThreshold );
        void    LogBackupThresholdExceeded( string libraryName, int currentThreshold );
        void    LogLibraryBackup( string libraryName );
        void    LogBackupException( Exception ex );
    }
}
