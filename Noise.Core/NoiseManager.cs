using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Core.DataBuilders;
using Noise.Core.FileStore;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core {
	public class NoiseManager : INoiseManager {
		private readonly IUnityContainer	mContainer;
		private	string						mDatabaseName;
		private string						mDatabaseLocation;
		private readonly ILog				mLog;
		private readonly IDatabaseManager	mDatabase;
		public	IDataProvider				DataProvider { get; private set; }
		public	IAudioPlayer				AudioPlayer { get; private set; }
		public	IPlayQueue					PlayQueue { get; private set; }
		public	IPlayHistory				PlayHistory { get; private set; }

		public NoiseManager( IUnityContainer container ) {
			mContainer = container;

			mDatabase = mContainer.Resolve<IDatabaseManager>();
			mContainer.RegisterInstance( typeof( IDatabaseManager ), mDatabase );
			DataProvider = mContainer.Resolve<IDataProvider>();
			AudioPlayer = mContainer.Resolve<IAudioPlayer>();
			PlayQueue = mContainer.Resolve<IPlayQueue>();
			PlayHistory = mContainer.Resolve<IPlayHistory>();

			mLog = new Log();
		}

		public bool Initialize() {
			mLog.LogMessage( "-------------------------" );

			LoadConfiguration();
			if( mDatabase.InitializeDatabase( mDatabaseLocation ) ) {
				mDatabase.OpenWithCreateDatabase( mDatabaseName );
			}

			mLog.LogMessage( "Initialized NoiseManager." );

			return ( true );
		}

		public void Explore() {
			var	folderExplorer = mContainer.Resolve<IFolderExplorer>();
			folderExplorer.SynchronizeDatabaseFolders();

			var	dataExplorer = mContainer.Resolve<IMetaDataExplorer>();
			dataExplorer.BuildMetaData();

			var statistics = new DatabaseStatistics( mDatabase );
			statistics.GatherStatistics();
		}

		private void LoadConfiguration() {
			mDatabaseLocation = "(local)";
			mDatabaseName = "Noise";
		}
	}
}
