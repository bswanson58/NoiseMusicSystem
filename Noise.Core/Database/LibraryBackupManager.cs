using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Noise.Core.Logging;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
    class LibraryBackupManager : ILibraryBackupManager, IRequireInitialization,
                                 IHandle<Events.LibraryBackupPressure> {
        private readonly ILibrarian             mLibrarian;
        private readonly ILibraryConfiguration  mLibraryConfiguration;
        private readonly ILibraryBuilder        mLibraryBuilder;
        private readonly IPreferences           mPreferences;
        private readonly IEventAggregator       mEventAggregator;
        private readonly ILogBackup             mLog;

        public LibraryBackupManager( ILifecycleManager lifecycleManager, ILibrarian librarian, ILibraryConfiguration configuration, ILibraryBuilder libraryBuilder,
                                     IPreferences preferences, IEventAggregator eventAggregator, ILogBackup log ) {
            mLibrarian = librarian;
            mLibraryConfiguration = configuration;
            mLibraryBuilder = libraryBuilder;
            mPreferences = preferences;
            mEventAggregator = eventAggregator;
            mLog = log;

            lifecycleManager.RegisterForInitialize( this );
            lifecycleManager.RegisterForShutdown( this );
        }

        public void Initialize() {
            mEventAggregator.Subscribe( this );
        }

        public void Shutdown() {
            mEventAggregator.Unsubscribe( this );
        }

        public async Task<bool> BackupLibrary( Action<LibrarianProgressReport> progress ) {
            var retValue = false;
            var library = mLibraryConfiguration.Current;

            if(( library != null ) &&
               (!mLibraryBuilder.LibraryUpdateInProgress )) {
                try {
                    var userState = new Events.LibraryUserState() { IsRestoring = false };

                    mEventAggregator.PublishOnUIThread( userState );

                    mLibraryConfiguration.Close( library );

                    retValue = await mLibrarian.BackupLibrary( library, progress );

                    library.BackupPressure = 0;
                    mLibraryConfiguration.UpdateConfiguration( library );

                    mLibraryConfiguration.Open( library );

                    userState.IsRestoring = true;
                    mEventAggregator.PublishOnUIThread( userState );

                    EnforceBackupCopies( library );

                    mLog.LogLibraryBackup( library.LibraryName );
                }
                catch( Exception ex ) {
                    mLog.LogBackupException( ex, "BackupLibrary" );
                }
            }

            return retValue;
        }

        private void EnforceBackupCopies( LibraryConfiguration library ) {
            try {
                var preferences = mPreferences.Load<NoiseCorePreferences>();

                if( preferences.EnforceBackupCopyLimit ) {
                    var backupCopies = new List<LibraryBackup>( from backup in mLibraryConfiguration.GetLibraryBackups( library ) orderby backup.BackupDate select backup );

                    while(( preferences.MaximumBackupCopies > 0 ) &&
                          ( backupCopies.Count > preferences.MaximumBackupCopies )) {
                        var oldestBackup = backupCopies.FirstOrDefault();

                        if( oldestBackup != null ) {
                            Directory.Delete( oldestBackup.BackupPath, true );

                            mLog.LogBackupDeleted( library.LibraryName, oldestBackup.BackupDate );
                        }

                        if( backupCopies.Any()) {
                            backupCopies.RemoveAt( 0 );
                        }
                    }
                }
            }
            catch( Exception ex ) {
                mLog.LogBackupException( ex, "EnforceBackupCopies" );
            }
        }

        public void Handle( Events.LibraryBackupPressure args ) {
            var config = mLibraryConfiguration.Current;

            if( config != null ) {
                config.BackupPressure += args.PressureAdded;

                mLibraryConfiguration.UpdateConfiguration( config );

                var preferences = mPreferences.Load<NoiseCorePreferences>();

                mLog.LogBackupPressure( args.PressureSource, config.BackupPressure, preferences.MaximumBackupPressure );

                if( config.BackupPressure > preferences.MaximumBackupPressure ) {
                    mEventAggregator.PublishOnUIThread( new Events.LibraryBackupPressureThreshold( Events.LibraryBackupPressureThreshold.ThresholdLevel.Exceeded ));

                    mLog.LogBackupThresholdExceeded( config.LibraryName, preferences.MaximumBackupPressure );
                }
            }
        }
    }
}
