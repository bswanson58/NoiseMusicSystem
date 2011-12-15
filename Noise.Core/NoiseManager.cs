using Microsoft.Practices.Prism.Events;
using Noise.Core.BackgroundTasks;
using Noise.Core.Database;
using Noise.Core.DataBuilders;
using Noise.Core.DataProviders;
using Noise.Core.FileStore;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.RemoteHost;

namespace Noise.Core {
	public class NoiseManager : INoiseManager {
		private	readonly IEventAggregator			mEvents;
		private readonly IDataUpdates				mDataUpdates;
		private readonly IContentManager			mContentManager;
		private readonly IFileUpdates				mFileUpdates;
		private readonly IBackgroundTaskManager		mBackgroundTaskMgr;
		private readonly ILyricsProvider			mLyricsProvider;
		private readonly IRemoteServer				mRemoteServer;

		public	IDatabaseManager			DatabaseManager { get; private set; }
		public	ICloudSyncManager			CloudSyncMgr { get; private set; }
		public	IDataProvider				DataProvider { get; private set; }
		public	ISearchProvider				SearchProvider { get; private set; }
		public	IPlayQueue					PlayQueue { get; private set; }
		public	IPlayHistory				PlayHistory { get; private set; }
		public	IPlayListMgr				PlayListMgr { get; private set; }
		public	IPlayController				PlayController { get; private set; }
		public	ILibraryBuilder				LibraryBuilder { get; private set; }
		public	ITagManager					TagManager { get; private set; }
		public	IDataExchangeManager		DataExchangeMgr { get; private set; }

		public bool							IsInitialized { get; set; }

		public NoiseManager( IEventAggregator eventAggregator,
							 IBackgroundTaskManager backgroundTaskManager,
							 IDatabaseManager databaseManager,
							 IDataProvider dataProvider,
							 ICloudSyncManager cloudSyncManager,
							 IDataExchangeManager dataExchangeManager,
							 IContentManager contentManager,
							 IDataUpdates dataUpdates,
							 IFileUpdates fileUpdates,
							 ILibraryBuilder libraryBuilder,
							 ILyricsProvider lyricsProvider,
							 ISearchProvider searchProvider,
							 IPlayQueue playQueue, 
							 IPlayHistory playHistory,
							 IPlayListMgr playListMgr,
							 IPlayController playController,
							 IRemoteServer remoteServer,
							 ITagManager tagManager ) {
			mEvents = eventAggregator;
			mBackgroundTaskMgr = backgroundTaskManager;
			mContentManager = contentManager;
			mDataUpdates = dataUpdates;
			mFileUpdates = fileUpdates;
			mLyricsProvider = lyricsProvider;
			mRemoteServer = remoteServer;
			DatabaseManager = databaseManager;
			DataExchangeMgr = dataExchangeManager;
			DataProvider = dataProvider;
			CloudSyncMgr = cloudSyncManager;
			LibraryBuilder = libraryBuilder;
			SearchProvider = searchProvider;
			PlayQueue = playQueue;
			PlayHistory = playHistory;
			PlayListMgr = playListMgr;
			PlayController = playController;
			TagManager = tagManager;
		}

		public bool Initialize() {
			NoiseLogger.Current.LogMessage( "Initializing Noise Music System" );

			if( DatabaseManager.Initialize()) {
				if(!TagManager.Initialize()) {
					NoiseLogger.Current.LogMessage( "Noise Manager: TagManager could not be initialized" );
				}

				if(!mDataUpdates.Initialize()) {
					NoiseLogger.Current.LogMessage( "Noise Manager: DataUpdates could not be initialized" );
				}

				if(!mFileUpdates.Initialize()) {
					NoiseLogger.Current.LogMessage( "Noise Manager: FileUpdates could not be initialized" );
				}

				if(!mBackgroundTaskMgr.Initialize( this )) {
					NoiseLogger.Current.LogMessage( "Noise Manager: BackgroundTaskManager cound not be initialized." );
				}

				var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<CloudSyncConfiguration>( CloudSyncConfiguration.SectionName );
				if( configuration != null ) {
					if( CloudSyncMgr.InitializeCloudSync( configuration.LoginName, configuration.LoginPassword )) {
						CloudSyncMgr.MaintainSynchronization = configuration.UseCloud;
					}
					else {
						NoiseLogger.Current.LogMessage( "Noise Manager: Could not initialize cloud sync." );
					}
				}

				if(!mLyricsProvider.Initialize()) {
					NoiseLogger.Current.LogMessage( "Noise Manager: Could not initialize lyrics provider." );
				}

				if(!PlayController.Initialize()) {
					NoiseLogger.Current.LogMessage( "NoiseManager: PlayController could not be initialized." );
				}

				if(!PlayHistory.Initialize()) {
					NoiseLogger.Current.LogMessage( "NoiseManager: PlayHistory could not be initialized." );
				}

				if(!mContentManager.Initialize( this )) {
					NoiseLogger.Current.LogMessage( "Noise Manager: ContentManager could not be initialized" );
				}

				var sysConfig = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );
				if(( sysConfig != null ) &&
				   ( sysConfig.EnableRemoteAccess )) {
					mRemoteServer.OpenRemoteServer();
				}

				NoiseLogger.Current.LogMessage( "Initialized NoiseManager." );

				IsInitialized = true;
			}
			else {
				NoiseLogger.Current.LogMessage( "Noise Manager: DatabaseManager could not be initialized" );
			}

			return ( IsInitialized );
		}

		public void Shutdown() {
			mEvents.GetEvent<Events.SystemShutdown>().Publish( this );

			mFileUpdates.Shutdown();
			mBackgroundTaskMgr.Stop();
			mRemoteServer.CloseRemoteServer();

			LibraryBuilder.StopLibraryUpdate();

			DatabaseManager.Shutdown();
		}

		public void StartExplorerJobs() {
			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

			if( configuration != null ) {
				if( configuration.EnableLibraryExplorer ) {
					LibraryBuilder.StartLibraryUpdate();
				}
				else {
					LibraryBuilder.LogLibraryStatistics();
				}

				LibraryBuilder.EnableUpdateOnLibraryChange = configuration.EnableLibraryChangeUpdates;
			}
		}
	}
}
