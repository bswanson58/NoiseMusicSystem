﻿using System;
using System.Threading;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Core.DataBuilders;
using Noise.Core.Exceptions;
using Noise.Core.FileStore;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Quartz;
using Quartz.Impl;

namespace Noise.Core {
	public class NoiseManager : INoiseManager {
		internal const string				cBackgroundContentExplorer = "BackgroundContentExplorer";

		private	readonly IUnityContainer	mContainer;
		private readonly IEventAggregator	mEvents;
		private readonly ILog				mLog;
		private readonly IDatabaseManager	mDatabase;
		private	readonly ISchedulerFactory	mSchedulerFactory;
		private	readonly IScheduler			mJobScheduler;
		private bool						mExploring;
		private bool						mContinueExploring;
		private IFolderExplorer				mFolderExplorer;
		private IMetaDataExplorer			mMetaDataExplorer;
		private	ISummaryBuilder				mSummaryBuilder;

		public IDataProvider DataProvider { get; private set; }
		public IAudioPlayer AudioPlayer { get; private set; }
		public IPlayQueue PlayQueue { get; private set; }
		public IPlayHistory PlayHistory { get; private set; }

		public NoiseManager( IUnityContainer container ) {
			mContainer = container;

			mLog = mContainer.Resolve<ILog>();
			mEvents = mContainer.Resolve<IEventAggregator>();
			mDatabase = mContainer.Resolve<IDatabaseManager>( Constants.NewInstance );
			mContainer.RegisterInstance( typeof( IDatabaseManager ), mDatabase );

			mSchedulerFactory = new StdSchedulerFactory();
			mJobScheduler = mSchedulerFactory.GetScheduler();
			mJobScheduler.Start();
		}

		public bool Initialize() {
			mLog.LogMessage( "-------------------------" );

			if( mDatabase.InitializeDatabase()) {
				mDatabase.OpenWithCreateDatabase();
			}
			else {
				mLog.LogMessage( "Noise Manager: Database could not be initialized" );
			}

			DataProvider = mContainer.Resolve<IDataProvider>();
			PlayQueue = mContainer.Resolve<IPlayQueue>();
			PlayHistory = mContainer.Resolve<IPlayHistory>();
			AudioPlayer = mContainer.Resolve<IAudioPlayer>();

			mLog.LogMessage( "Initialized NoiseManager." );

			StartExplorerJobs();

			return ( true );
		}

		public void Shutdown() {
			mJobScheduler.Shutdown( true );

			StopExploring();
			WaitForExplorer();

			if( mDatabase != null ) {
				mDatabase.CloseDatabase();
			}
		}

		private void WaitForExplorer() {
			int    timeOutSeconds = 10 * 2;

			while(( mExploring ) &&
				  ( timeOutSeconds > 0 )) {
				Thread.Sleep( TimeSpan.FromMilliseconds( 500 ));
				timeOutSeconds--;
			}
		}

		private void StartExplorerJobs() {
			var	systemConfig = mContainer.Resolve<ISystemConfiguration>();
			var configuration = systemConfig.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

			if( configuration != null ) {
				if( configuration.EnableLibraryExplorer ) {
					StartLibraryExplorer();
				}

				if( configuration.EnableBackgroundContentExplorer ) {
					StartBackgroundContentExplorer();
				}
			}
			
		}

		private void StartLibraryExplorer() {
			mLog.LogMessage( "Starting Library Explorer." );

			ThreadPool.QueueUserWorkItem( UpdateLibrary );
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

		private void UpdateLibrary( object state ) {
			mContinueExploring = true;
			mExploring = true;

			try {
				if( mContinueExploring ) {
					try {
						mFolderExplorer = mContainer.Resolve<IFolderExplorer>();
						mFolderExplorer.SynchronizeDatabaseFolders();
					}
					catch( StorageConfigurationException ) {
						InitializeStorageConfiguration();
					}
				}

				var results = new DatabaseChangeSummary();
				if( mContinueExploring ) {
					mMetaDataExplorer = mContainer.Resolve<IMetaDataExplorer>();
					mMetaDataExplorer.BuildMetaData( results );
				}

				if(( mContinueExploring ) &&
				   ( results.HaveChanges )) {
					mSummaryBuilder = mContainer.Resolve<ISummaryBuilder>();
					mSummaryBuilder.BuildSummaryData();
				}

				DatabaseStatistics	statistics = null;
				if( mContinueExploring ) {
					statistics = new DatabaseStatistics( mDatabase );
					statistics.GatherStatistics();
				}

				mLog.LogMessage( "Explorer Finished." );

				if( results.HaveChanges ) {
					mEvents.GetEvent<Events.DatabaseChanged>().Publish( results );
					mLog.LogInfo( string.Format( "Database changes: {0}", results ) );
				}

				if( statistics != null ) {
					mLog.LogInfo( statistics.ToString() );
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Folder Explorer error: ", ex );
			}
			finally {
				mFolderExplorer = null;
				mMetaDataExplorer = null;
				mSummaryBuilder = null;
				mExploring = false;
			}
		}

		private void StopExploring() {
			mContinueExploring = false;

			if( mFolderExplorer != null ) {
				mFolderExplorer.Stop();
			}

			if( mMetaDataExplorer != null ) {
				mMetaDataExplorer.Stop();
			}

			if( mSummaryBuilder != null ) {
				mSummaryBuilder.Stop();
			}
		}

		private void InitializeStorageConfiguration() {
			var configMgr = mContainer.Resolve<ISystemConfiguration>();
			var storageConfig = configMgr.RetrieveConfiguration<StorageConfiguration>( StorageConfiguration.SectionName );

			if( storageConfig.RootFolders.Count == 0 ) {
				var rootFolder = new RootFolderConfiguration { Path = @"D:\Music", Description = "Music Folder", PreferFolderStrategy = true };

				rootFolder.StorageStrategy.Add( new FolderStrategyConfiguration( 0, eFolderStrategy.Artist ));
				rootFolder.StorageStrategy.Add( new FolderStrategyConfiguration( 1, eFolderStrategy.Album ));
				rootFolder.StorageStrategy.Add( new FolderStrategyConfiguration( 2, eFolderStrategy.Volume ));

				storageConfig.RootFolders.Add( rootFolder );
				configMgr.Save( storageConfig );

				var explorerConfig = configMgr.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

				explorerConfig.EnableLibraryExplorer = true;
				explorerConfig.EnableBackgroundContentExplorer = true;
				configMgr.Save( explorerConfig );
			}
		}
	}
}
