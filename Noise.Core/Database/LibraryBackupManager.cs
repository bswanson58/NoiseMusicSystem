using System;
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
        private readonly IPreferences           mPreferences;
        private readonly IEventAggregator       mEventAggregator;
        private readonly ILogBackup             mLog;

        public LibraryBackupManager( ILifecycleManager lifecycleManager, ILibrarian librarian, ILibraryConfiguration configuration,
                                     IPreferences preferences, IEventAggregator eventAggregator, ILogBackup log ) {
            mLibrarian = librarian;
            mLibraryConfiguration = configuration;
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

        public async Task BackupLibrary( Action<LibrarianProgressReport> progress ) {
            var library = mLibraryConfiguration.Current;

            if( library != null ) {
                mLibraryConfiguration.Close( library );

                await mLibrarian.BackupLibrary( library, progress );

                library.BackupPressure = 0;
                mLibraryConfiguration.UpdateConfiguration( library );

                mLibraryConfiguration.Open( library );

                mLog.LogLibraryBackup( library.LibraryName );
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
