using System;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Noise.Core.BackgroundTasks;
using Noise.Core.Database;
using Noise.Core.DataProviders;
using Noise.Core.FileStore;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core {
	public class NoiseManager : INoiseManager {
		private	readonly IUnityContainer	mContainer;
		private readonly IDatabaseManager	mDatabaseManager;
		private	readonly IEventAggregator	mEvents;
		private readonly ILog				mLog;
		private	IPlayController				mPlayController;
		private IDataUpdates				mDataUpdates;
		private IFileUpdates				mFileUpdates;
		private IBackgroundTaskManager		mBackgroundTaskMgr;
		private ILyricsProvider				mLyricsProvider;

		public	ICloudSyncManager			CloudSyncMgr { get; private set; }
		public	IDataProvider				DataProvider { get; private set; }
		public	ISearchProvider				SearchProvider { get; private set; }
		public	IPlayQueue					PlayQueue { get; private set; }
		public	IPlayHistory				PlayHistory { get; private set; }
		public	IPlayListMgr				PlayListMgr { get; private set; }
		public	ILibraryBuilder				LibraryBuilder { get; private set; }
		public	ITagManager					TagManager { get; private set; }
		public	IDataExchangeManager		DataExchangeMgr { get; private set; }

		public bool							IsInitialized { get; set; }

		public NoiseManager( IUnityContainer container ) {
			mContainer = container;

			mLog = mContainer.Resolve<ILog>();
			mEvents = mContainer.Resolve<IEventAggregator>();
			mDatabaseManager = mContainer.Resolve<IDatabaseManager>( Constants.NewInstance );
			mContainer.RegisterInstance( mDatabaseManager );
		}

		public bool Initialize() {
			mLog.LogMessage( "---------------------------" );
			mLog.LogMessage( "Starting Noise Music System" );

			if( mDatabaseManager.Initialize()) {
				DataProvider = mContainer.Resolve<IDataProvider>();
				LibraryBuilder = mContainer.Resolve<ILibraryBuilder>();
				SearchProvider = mContainer.Resolve<ISearchProvider>();
				TagManager = mContainer.Resolve<ITagManager>();
				DataExchangeMgr = mContainer.Resolve<IDataExchangeManager>();

				PlayQueue = mContainer.Resolve<IPlayQueue>();
				PlayHistory = mContainer.Resolve<IPlayHistory>();
				PlayListMgr = mContainer.Resolve<IPlayListMgr>();

				mDataUpdates = mContainer.Resolve<IDataUpdates>();
				if(!mDataUpdates.Initialize()) {
					mLog.LogMessage( "Noise Manager: DataUpdates could not be initialized" );
				}

				mFileUpdates = mContainer.Resolve<IFileUpdates>();
				if(!mFileUpdates.Initialize()) {
					mLog.LogMessage( "Noise Manager: FileUpdates could not be initialized" );
				}

				mBackgroundTaskMgr = mContainer.Resolve<IBackgroundTaskManager>();
				if(!mBackgroundTaskMgr.Initialize()) {
					mLog.LogMessage( "Noise Manager: BackgroundTaskManager cound not be initialized." );
				}

				CloudSyncMgr = mContainer.Resolve<ICloudSyncManager>();
				var	systemConfig = mContainer.Resolve<ISystemConfiguration>();
				var configuration = systemConfig.RetrieveConfiguration<CloudSyncConfiguration>( CloudSyncConfiguration.SectionName );

				if( configuration != null ) {
					if( CloudSyncMgr.InitializeCloudSync( configuration.LoginName, configuration.LoginPassword )) {
						CloudSyncMgr.MaintainSynchronization = configuration.UseCloud;
					}
					else {
						mLog.LogMessage( "Noise Manager: Could not initialize cloud sync." );
					}
				}

				mLyricsProvider = mContainer.Resolve<ILyricsProvider>();
				if(!mLyricsProvider.Initialize()) {
					mLog.LogMessage( "Noise Manager: Could not initialize lyrics provider." );
				}

				mLog.LogMessage( "Initialized NoiseManager." );

				IsInitialized = true;
			}
			else {
				mLog.LogMessage( "Noise Manager: DatabaseManager could not be initialized" );
			}

			return ( IsInitialized );
		}

		public void ConfigurationChanged() {
			var folderExplorer = mContainer.Resolve<IFolderExplorer>();
			var database = mDatabaseManager.ReserveDatabase();

			try {
				folderExplorer.LoadConfiguration( database );
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - NoiseManager:ConfigurationChanged", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}
		}

		public void Shutdown() {
			mEvents.GetEvent<Events.SystemShutdown>().Publish( this );

			mFileUpdates.Shutdown();
			mBackgroundTaskMgr.Stop();

			LibraryBuilder.StopLibraryUpdate();

			mDatabaseManager.Shutdown();
		}

		public IPlayController PlayController {
			get {
				if( mPlayController == null ) {
					mPlayController = mContainer.Resolve<IPlayController>();
				}

				return( mPlayController );
			}
		}

		public void StartExplorerJobs() {
			var	systemConfig = mContainer.Resolve<ISystemConfiguration>();
			var configuration = systemConfig.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

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
