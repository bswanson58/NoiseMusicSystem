using System.Threading;
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
		private readonly ILog				mLog;
		private readonly IDatabaseManager	mDatabase;
		private bool						mContinueExploring;
		public	IDataProvider				DataProvider { get; private set; }
		public	IAudioPlayer				AudioPlayer { get; private set; }
		public	IPlayQueue					PlayQueue { get; private set; }
		public	IPlayHistory				PlayHistory { get; private set; }

		public NoiseManager( IUnityContainer container ) {
			mContainer = container;

			mLog = mContainer.Resolve<ILog>();
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

		public void StartExploring() {
			mContinueExploring = true;
			mLog.LogMessage( "Starting Explorer." );

			ThreadPool.QueueUserWorkItem( Explore );
		}

		public void StopExploring() {
			mContinueExploring = false;
		}

		private void Explore(object state ) {
			if( mContinueExploring ) {
				try {
					var	folderExplorer = mContainer.Resolve<IFolderExplorer>();

					folderExplorer.SynchronizeDatabaseFolders();
				}
				catch( StorageConfigurationException ) {
					InitializeStorageConfiguration();
				}
			}

			if( mContinueExploring ) {
				var	dataExplorer = mContainer.Resolve<IMetaDataExplorer>();
				dataExplorer.BuildMetaData();
			}

			if( mContinueExploring ) {
				var summaryBuilder = mContainer.Resolve<ISummaryBuilder>();
				summaryBuilder.BuildSummaryData( mDatabase );
			}

			if( mContinueExploring ) {
				var statistics = new DatabaseStatistics( mDatabase );
				statistics.GatherStatistics();
			}

			mLog.LogMessage( "Explorer Finished." );
		}

		private void InitializeStorageConfiguration() {
			var configMgr = mContainer.Resolve<ISystemConfiguration>();
			var storageConfig = configMgr.RetrieveConfiguration<StorageConfiguration>( StorageConfiguration.SectionName );

			if( storageConfig.RootFolders.Count == 0 ) {
				var rootFolder = new RootFolderConfiguration { Path = @"D:\Music", Description = "Music Folder" };

				rootFolder.PreferFolderStrategy = true;
				rootFolder.StorageStrategy.Add( new FolderStrategyConfiguration( 0, eFolderStrategy.Artist ));
				rootFolder.StorageStrategy.Add( new FolderStrategyConfiguration( 1, eFolderStrategy.Album  ));
				rootFolder.StorageStrategy.Add( new FolderStrategyConfiguration( 2, eFolderStrategy.Volume ));

				storageConfig.RootFolders.Add( rootFolder );
				configMgr.Save( storageConfig );
			}
		}
	}
}
