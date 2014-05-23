using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.RemoteHost;
using ReusableBits.Mvvm.CaliburnSupport;

namespace Noise.Core {
	public class NoiseManager : INoiseManager, IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing> {
		private	readonly IEventAggregator		mEvents;
		private readonly ILifecycleManager		mLifecycleManager;
		private readonly IPreferences			mPreferences;
		private readonly IRemoteServer			mRemoteServer;
		private readonly ICloudSyncManager		mCloudSyncMgr;
		private readonly ILibraryBuilder		mLibraryBuilder;
		private readonly IDatabaseManager		mDatabaseManager;

		public NoiseManager( IEventAggregator eventAggregator,
							 ILifecycleManager lifecycleManager,
							 IDatabaseManager databaseManager,
							 ICloudSyncManager cloudSyncManager,
							 ILibraryBuilder libraryBuilder,
							 IRemoteServer remoteServer,
							 IPreferences preferences,
							 // components that just need to be referrenced.
							 IAudioController audioController,
							 IPlayController playController,
							 ISearchProvider searchProvider,
							 ITagManager tagManager,
							 IMetadataManager metadataManager,
							 IEnumerable<IRequireConstruction> backgroundComponents ) {
			mEvents = eventAggregator;
			mLifecycleManager = lifecycleManager;
			mRemoteServer = remoteServer;
			mDatabaseManager = databaseManager;
			mCloudSyncMgr = cloudSyncManager;
			mLibraryBuilder = libraryBuilder;
			mPreferences = preferences;
		}

		public async Task<bool> AsyncInitialize() {
			var initTask = new Task<bool>(() => ( InitializeAndNotify()));

			initTask.Start();

			return( await initTask );
		}

		public bool InitializeAndNotify() {
			var retValue = Initialize();

			mEvents.PublishOnUIThreadAsync( new Events.NoiseSystemReady( this, retValue ));

			return( retValue );
		}
		
		public bool Initialize() {
			var isInitialized = false;

			NoiseLogger.Current.LogMessage( "Initializing Noise Music System" );

			try {
				mLifecycleManager.Initialize();
				mEvents.Subscribe( this );

				var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<CloudSyncConfiguration>( CloudSyncConfiguration.SectionName );
				if( configuration != null ) {
					if( mCloudSyncMgr.InitializeCloudSync( configuration.LoginName, configuration.LoginPassword )) {
						mCloudSyncMgr.MaintainSynchronization = configuration.UseCloud;
					}
					else {
						NoiseLogger.Current.LogMessage( "Noise Manager: Could not initialize cloud sync." );
					}
				}

				var preferences = mPreferences.Load<NoiseCorePreferences>();

				if( preferences.EnableRemoteAccess ) {
					mRemoteServer.OpenRemoteServer();
				}

				isInitialized = true;

				NoiseLogger.Current.LogMessage( "Initialized NoiseManager." );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "NoiseManager:Initialize", ex );
			}

			return ( isInitialized );
		}

		public void Shutdown() {
			mEvents.Publish( new Events.SystemShutdown());

			mRemoteServer.CloseRemoteServer();
			mLibraryBuilder.StopLibraryUpdate();

			mLifecycleManager.Shutdown();

			mDatabaseManager.Shutdown();
		}

		public void Handle( Events.DatabaseOpened arg ) {
			mLibraryBuilder.LogLibraryStatistics();
		}

		public void Handle( Events.DatabaseClosing args ) {
			mLibraryBuilder.StopLibraryUpdate();
		}
	}
}
