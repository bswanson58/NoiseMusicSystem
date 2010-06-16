﻿using Microsoft.Practices.Unity;
using Noise.Core.FileStore;
using Noise.Infrastructure;

namespace Noise.Core {
	public class NoiseManager : INoiseManager {
		private readonly IUnityContainer	mContainer;
		private	string						mDatabaseName;
		private string						mDatabaseLocation;
		private readonly ILog				mLog;
		private readonly IDatabaseManager	mDatabase;

		public NoiseManager( IUnityContainer container ) {
			mContainer = container;

			mDatabase = mContainer.Resolve<IDatabaseManager>();
			mContainer.RegisterInstance( typeof( IDatabaseManager ), mDatabase );

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
			var		explorer = mContainer.Resolve<IFolderExplorer>();

			explorer.SynchronizeDatabaseFolders();
		}

		private void LoadConfiguration() {
			mDatabaseLocation = "(local)";
			mDatabaseName = "Noise";
		}
	}
}
