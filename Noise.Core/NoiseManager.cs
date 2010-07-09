using System.Threading;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Core.DataBuilders;
using Noise.Core.FileStore;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core {
	public class NoiseManager : INoiseManager {
		private	readonly IUnityContainer	mContainer;
		private	string						mDatabaseName;
		private string						mDatabaseLocation;
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
			mDatabase = mContainer.Resolve<IDatabaseManager>();
			mContainer.RegisterInstance( typeof( IDatabaseManager ), mDatabase );
			DataProvider = mContainer.Resolve<IDataProvider>();
			AudioPlayer = mContainer.Resolve<IAudioPlayer>();
			PlayQueue = mContainer.Resolve<IPlayQueue>();
		}

		public bool Initialize() {
			mLog.LogMessage( "-------------------------" );

			LoadConfiguration();
			if( mDatabase.InitializeDatabase( mDatabaseLocation ) ) {
				mDatabase.OpenWithCreateDatabase( mDatabaseName );
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
				var	folderExplorer = mContainer.Resolve<IFolderExplorer>();
				folderExplorer.SynchronizeDatabaseFolders();
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

		private void LoadConfiguration() {
			mDatabaseLocation = "(local)";
			mDatabaseName = "Noise";
		}
	}
}
