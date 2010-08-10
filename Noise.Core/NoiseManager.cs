using System;
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

namespace Noise.Core {
	public class NoiseManager : INoiseManager {
		private	readonly IUnityContainer	mContainer;
		private readonly IEventAggregator	mEvents;
		private readonly ILog				mLog;
		private readonly IDatabaseManager	mDatabase;
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

			return ( true );
		}

		public void Shutdown() {
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

		public void StartExploring() {
			var	systemConfig = mContainer.Resolve<ISystemConfiguration>();
			var configuration = systemConfig.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

			if(( configuration != null ) &&
			   ( configuration.EnableExplorer )) {
				mContinueExploring = true;
				mLog.LogMessage( "Starting Explorer." );

				ThreadPool.QueueUserWorkItem( Explore );
			}
		}

		public void StopExploring() {
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

		private void Explore( object state ) {
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

				explorerConfig.EnableExplorer = false;
				configMgr.Save( explorerConfig );
			}
		}
	}
}
