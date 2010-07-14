using System.Threading;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Core.DataBuilders;
using Noise.Core.Exceptions;
using Noise.Core.FileStore;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
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
			DataProvider = mContainer.Resolve<IDataProvider>();
			AudioPlayer = mContainer.Resolve<IAudioPlayer>();
			PlayQueue = mContainer.Resolve<IPlayQueue>();
		}

		public bool Initialize() {
			mLog.LogMessage( "-------------------------" );

			if( mDatabase.InitializeDatabase()) {
				mDatabase.OpenWithCreateDatabase();
			}

			mLog.LogMessage( "Initialized NoiseManager." );

			PlayHistory = mContainer.Resolve<IPlayHistory>();

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
				catch( StorageConfigurationException ex ) {
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
				var rootFolder = new RootFolderConfiguration { Path = @"D:\Music" };

				storageConfig.RootFolders.Add( rootFolder );
				configMgr.Save( storageConfig );
			}
		}
	}
}
