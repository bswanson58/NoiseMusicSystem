using Microsoft.Practices.Prism.Events;
using Noise.Core.BackgroundTasks;
using Noise.Core.Database;
using Noise.Core.DataBuilders;
using Noise.Core.DataProviders;
using Noise.Core.FileStore;
using Noise.Core.Support;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.RemoteHost;

namespace Noise.Core {
	public class NoiseManager : INoiseManager {
		private	readonly IEventAggregator			mEvents;
		private readonly ILifecycleManager			mLifecycleManager;
		private readonly IContentManager			mContentManager;
		private readonly IBackgroundTaskManager		mBackgroundTaskMgr;
		private readonly IDataUpdates				mDataUpdates;
		private readonly IFileUpdates				mFileUpdates;
		private readonly ILyricsProvider			mLyricsProvider;
		private readonly IRemoteServer				mRemoteServer;
		private readonly ICloudSyncManager			mCloudSyncMgr;
		private readonly ILibraryBuilder			mLibraryBuilder;
		private readonly IPlayController			mPlayController;

		public	IDatabaseManager			DatabaseManager { get; private set; }
		public	IDataProvider				DataProvider { get; private set; }
		public	ISearchProvider				SearchProvider { get; private set; }
		public	ITagManager					TagManager { get; private set; }

		public NoiseManager( IEventAggregator eventAggregator,
							 ILifecycleManager lifecycleManager,
							 IBackgroundTaskManager backgroundTaskManager,
							 IDatabaseManager databaseManager,
							 IDataProvider dataProvider,
							 IDataUpdates dataUpdates,
							 IFileUpdates fileUpdates,
							 ICloudSyncManager cloudSyncManager,
							 IContentManager contentManager,
							 ILibraryBuilder libraryBuilder,
							 ILyricsProvider lyricsProvider,
							 ISearchProvider searchProvider,
							 IPlayController playController,
							 IRemoteServer remoteServer,
							 ITagManager tagManager ) {
			mEvents = eventAggregator;
			mLifecycleManager = lifecycleManager;
			mBackgroundTaskMgr = backgroundTaskManager;
			mContentManager = contentManager;
			mDataUpdates = dataUpdates;
			mFileUpdates = fileUpdates;
			mLyricsProvider = lyricsProvider;
			mRemoteServer = remoteServer;
			DatabaseManager = databaseManager;
			DataProvider = dataProvider;
			mCloudSyncMgr = cloudSyncManager;
			mLibraryBuilder = libraryBuilder;
			SearchProvider = searchProvider;
			mPlayController = playController;
			TagManager = tagManager;
		}

		public bool Initialize() {
			var isInitialized = false;

			NoiseLogger.Current.LogMessage( "Initializing Noise Music System" );

			if( DatabaseManager.Initialize()) {
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

			DatabaseManager.Shutdown();
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
