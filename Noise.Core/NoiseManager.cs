using System;
using Microsoft.Practices.Prism.Events;
using Noise.Core.BackgroundTasks;
using Noise.Core.Database;
using Noise.Core.DataProviders;
using Noise.Core.FileStore;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core {
	public class NoiseManager : INoiseManager {
		private	readonly IEventAggregator			mEvents;
		private readonly IDataUpdates				mDataUpdates;
		private readonly IFolderExplorer			mFolderExplorer;
		private readonly IFileUpdates				mFileUpdates;
		private readonly IBackgroundTaskManager		mBackgroundTaskMgr;
		private readonly ILyricsProvider			mLyricsProvider;

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
							 IDataUpdates dataUpdates,
							 IFileUpdates fileUpdates,
							 IFolderExplorer folderExplorer,
							 ILibraryBuilder libraryBuilder,
							 ILyricsProvider lyricsProvider,
							 ISearchProvider searchProvider,
							 IPlayQueue playQueue, 
							 IPlayHistory playHistory,
							 IPlayListMgr playListMgr,
							 IPlayController playController,
							 ITagManager tagManager ) {
			mEvents = eventAggregator;
			mBackgroundTaskMgr = backgroundTaskManager;
			mDataUpdates = dataUpdates;
			mFileUpdates = fileUpdates;
			mFolderExplorer = folderExplorer;
			mLyricsProvider = lyricsProvider;
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
			NoiseLogger.Current.LogMessage( "---------------------------" );
			NoiseLogger.Current.LogMessage( "Starting Noise Music System" );

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

				if(!mBackgroundTaskMgr.Initialize()) {
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

				NoiseLogger.Current.LogMessage( "Initialized NoiseManager." );

				IsInitialized = true;
			}
			else {
				NoiseLogger.Current.LogMessage( "Noise Manager: DatabaseManager could not be initialized" );
			}

			return ( IsInitialized );
		}

		public void ConfigurationChanged() {
			var database = DatabaseManager.ReserveDatabase();

			try {
				mFolderExplorer.LoadConfiguration( database );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - NoiseManager:ConfigurationChanged", ex );
			}
			finally {
				DatabaseManager.FreeDatabase( database );
			}
		}

		public void Shutdown() {
			mEvents.GetEvent<Events.SystemShutdown>().Publish( this );

			mFileUpdates.Shutdown();
			mBackgroundTaskMgr.Stop();

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
