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
		private readonly ICloudSyncManager			mCloudSyncMgr;
		private readonly ILibraryBuilder			mLibraryBuilder;
		private readonly IPlayHistory				mPlayHistory;
		private readonly IPlayController			mPlayController;

		public	IDatabaseManager			DatabaseManager { get; private set; }
		public	IDataProvider				DataProvider { get; private set; }
		public	ISearchProvider				SearchProvider { get; private set; }
		public	ITagManager					TagManager { get; private set; }

		public NoiseManager( IEventAggregator eventAggregator,
							 IBackgroundTaskManager backgroundTaskManager,
							 IDatabaseManager databaseManager,
							 IDataProvider dataProvider,
							 ICloudSyncManager cloudSyncManager,
							 IContentManager contentManager,
							 IDataUpdates dataUpdates,
							 IFileUpdates fileUpdates,
							 ILibraryBuilder libraryBuilder,
							 ILyricsProvider lyricsProvider,
							 ISearchProvider searchProvider,
							 IPlayHistory playHistory,
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
			DataProvider = dataProvider;
			mCloudSyncMgr = cloudSyncManager;
			mLibraryBuilder = libraryBuilder;
			SearchProvider = searchProvider;
			mPlayHistory = playHistory;
			mPlayController = playController;
			TagManager = tagManager;
		}

		public bool Initialize() {
			var isInitialized = false;

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
					if( mCloudSyncMgr.InitializeCloudSync( configuration.LoginName, configuration.LoginPassword )) {
						mCloudSyncMgr.MaintainSynchronization = configuration.UseCloud;
					}
					else {
						NoiseLogger.Current.LogMessage( "Noise Manager: Could not initialize cloud sync." );
					}
				}

				if(!mLyricsProvider.Initialize()) {
					NoiseLogger.Current.LogMessage( "Noise Manager: Could not initialize lyrics provider." );
				}

				if(!mPlayController.Initialize()) {
					NoiseLogger.Current.LogMessage( "NoiseManager: PlayController could not be initialized." );
				}

				if(!mPlayHistory.Initialize()) {
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

			mFileUpdates.Shutdown();
			mBackgroundTaskMgr.Stop();
			mRemoteServer.CloseRemoteServer();

			mLibraryBuilder.StopLibraryUpdate();

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
