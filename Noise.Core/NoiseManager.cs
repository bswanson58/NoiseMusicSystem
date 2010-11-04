using System;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Core.DataBuilders;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;
using Quartz;
using Quartz.Impl;

namespace Noise.Core {
	public class NoiseManager : INoiseManager {
		internal const string				cBackgroundContentExplorer = "BackgroundContentExplorer";

		private	readonly IUnityContainer	mContainer;
		private readonly IDatabaseManager	mDatabaseManager;
		private readonly ILog				mLog;
		private	readonly ISchedulerFactory	mSchedulerFactory;
		private	readonly IScheduler			mJobScheduler;
		private	IPlayController				mPlayController;

		public	IDataProvider				DataProvider { get; private set; }
		public	ISearchProvider				SearchProvider { get; private set; }
		public	IPlayQueue					PlayQueue { get; private set; }
		public	IPlayHistory				PlayHistory { get; private set; }
		public	IPlayListMgr				PlayListMgr { get; private set; }
		public	ILibraryBuilder				LibraryBuilder { get; private set; }
		public	ITagManager					TagManager { get; private set; }

		public bool							IsInitialized { get; set; }

		public NoiseManager( IUnityContainer container ) {
			mContainer = container;

			mLog = mContainer.Resolve<ILog>();
			mDatabaseManager = mContainer.Resolve<IDatabaseManager>( Constants.NewInstance );
			mContainer.RegisterInstance( mDatabaseManager );

			mSchedulerFactory = new StdSchedulerFactory();
			mJobScheduler = mSchedulerFactory.GetScheduler();
			mJobScheduler.Start();
		}

		public bool Initialize() {
			mLog.LogMessage( "-------------------------" );

			if( mDatabaseManager.Initialize()) {
				DataProvider = mContainer.Resolve<IDataProvider>();
				LibraryBuilder = mContainer.Resolve<ILibraryBuilder>();
				SearchProvider = mContainer.Resolve<ISearchProvider>();
				TagManager = mContainer.Resolve<ITagManager>();

				PlayQueue = mContainer.Resolve<IPlayQueue>();
				PlayHistory = mContainer.Resolve<IPlayHistory>();
				PlayListMgr = mContainer.Resolve<IPlayListMgr>();

				mLog.LogMessage( "Initialized NoiseManager." );

				StartExplorerJobs();

				IsInitialized = true;
			}
			else {
				mLog.LogMessage( "Noise Manager: DatabaseManager could not be initialized" );
			}

			return ( IsInitialized );
		}

		public void Shutdown() {
			mJobScheduler.Shutdown( true );

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

		private void StartExplorerJobs() {
			var	systemConfig = mContainer.Resolve<ISystemConfiguration>();
			var configuration = systemConfig.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

			if( configuration != null ) {
				if( configuration.EnableLibraryExplorer ) {
					LibraryBuilder.StartLibraryUpdate();
				}
				else {
					LibraryBuilder.LogLibraryStatistics();
				}

				if( configuration.EnableBackgroundContentExplorer ) {
					StartBackgroundContentExplorer();
				}
			}
		}

		private void StartBackgroundContentExplorer() {
			var jobDetail = new JobDetail( cBackgroundContentExplorer, "Explorer", typeof( BackgroundContentExplorerJob ));
			var trigger = new SimpleTrigger( cBackgroundContentExplorer, "Explorer",
											 DateTime.UtcNow + TimeSpan.FromMinutes( 1 ), null, SimpleTrigger.RepeatIndefinitely, TimeSpan.FromMinutes( 1 )); 
			var explorer = new BackgroundContentExplorer( mContainer );

			if( explorer.Initialize()) {
				trigger.JobDataMap[cBackgroundContentExplorer] = explorer;

				mJobScheduler.ScheduleJob( jobDetail, trigger );
				mLog.LogMessage( "Starting Background Content Explorer." );
			}
			else {
				mLog.LogInfo( "BackgroundContentExplorer could not be initialized." );
			}
		}
	}
}
