using System.Collections.Generic;
using Microsoft.Practices.Prism.Events;
using Noise.Core.Support;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.RemoteHost;

namespace Noise.Core {
	public class NoiseManager : INoiseManager {
		private	readonly IEventAggregator			mEvents;
		private readonly ILifecycleManager			mLifecycleManager;
		private readonly IRemoteServer				mRemoteServer;
		private readonly ICloudSyncManager			mCloudSyncMgr;
		private readonly ILibraryBuilder			mLibraryBuilder;
		private readonly IDatabaseManager			mDatabaseManager;

		public NoiseManager( IEventAggregator eventAggregator,
							 ILifecycleManager lifecycleManager,
							 IDatabaseManager databaseManager,
							 ICloudSyncManager cloudSyncManager,
							 ILibraryBuilder libraryBuilder,
							 IRemoteServer remoteServer,
							 // component that just need to be referrenced.
							 IDataProvider dataProvider,
							 IPlayController playController,
							 ISearchProvider searchProvider,
							 ITagManager tagManager,
							 IEnumerable<IRequireConstruction> backgroundComponents ) {
			mEvents = eventAggregator;
			mLifecycleManager = lifecycleManager;
			mRemoteServer = remoteServer;
			mDatabaseManager = databaseManager;
			mCloudSyncMgr = cloudSyncManager;
			mLibraryBuilder = libraryBuilder;
		}

		public bool Initialize() {
			var isInitialized = false;

			NoiseLogger.Current.LogMessage( "Initializing Noise Music System" );

			if( mDatabaseManager.Initialize()) {
				mLifecycleManager.Initialize();

				var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<CloudSyncConfiguration>( CloudSyncConfiguration.SectionName );
				if( configuration != null ) {
					if( mCloudSyncMgr.InitializeCloudSync( configuration.LoginName, configuration.LoginPassword )) {
						mCloudSyncMgr.MaintainSynchronization = configuration.UseCloud;
					}
					else {
						NoiseLogger.Current.LogMessage( "Noise Manager: Could not initialize cloud sync." );
					}
				}

				var sysConfig = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );
				if(( sysConfig != null ) &&
				   ( sysConfig.EnableRemoteAccess )) {
					mRemoteServer.OpenRemoteServer();
				}

				isInitialized = true;

				NoiseLogger.Current.LogMessage( "Initialized NoiseManager." );
			}
			else {
				NoiseLogger.Current.LogMessage( "Noise Manager: DatabaseManager could not be initialized" );
			}

			return ( isInitialized );
		}

		public void Shutdown() {
			mEvents.GetEvent<Events.SystemShutdown>().Publish( this );

			mRemoteServer.CloseRemoteServer();
			mLibraryBuilder.StopLibraryUpdate();

			mLifecycleManager.Shutdown();

			mDatabaseManager.Shutdown();
		}

		public void StartExplorerJobs() {
			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

			if( configuration != null ) {
				if( configuration.EnableLibraryExplorer ) {
					mLibraryBuilder.StartLibraryUpdate();
				}
				else {
					mLibraryBuilder.LogLibraryStatistics();
				}

				mLibraryBuilder.EnableUpdateOnLibraryChange = configuration.EnableLibraryChangeUpdates;
			}
		}
	}
}
