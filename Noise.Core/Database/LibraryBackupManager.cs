using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
    class LibraryBackupManager : ILibraryBackupManager, IRequireInitialization,
                                 IHandle<Events.LibraryBackupPressure> {
        private readonly ILibraryConfiguration  mLibraryConfiguration;
        private readonly IPreferences           mPreferences;
        private readonly IEventAggregator       mEventAggregator;
        private readonly INoiseLog              mLog;

        public LibraryBackupManager( ILifecycleManager lifecycleManager, ILibraryConfiguration configuration, IPreferences preferences, IEventAggregator eventAggregator, INoiseLog log ) {
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

        public void Handle( Events.LibraryBackupPressure args ) {
            var config = mLibraryConfiguration.Current;

            if( config != null ) {
                config.BackupPressure += args.PressureAdded;

                mLibraryConfiguration.UpdateConfiguration( config );

                var preferences = mPreferences.Load<NoiseCorePreferences>();

                if( config.BackupPressure > preferences.MaximumBackupPressure ) {
                    mEventAggregator.PublishOnUIThread( new Events.LibraryBackupPressureThreshold( Events.LibraryBackupPressureThreshold.ThresholdLevel.Exceeded ));

                    mLog.LogMessage( $"Library ({config.LibraryName}) backup pressure exceeded." );
                }
            }
        }
    }
}
