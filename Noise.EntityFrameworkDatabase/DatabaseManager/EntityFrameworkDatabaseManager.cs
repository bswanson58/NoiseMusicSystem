using System;
using System.IO;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DatabaseManager {
	public class EntityFrameworkDatabaseManager : IDatabaseManager {
		private const Int16		cDatabaseVersionMajor = 0;
		private const Int16		cDatabaseVersionMinor = 5;

		private readonly IDatabaseInitializeStrategy	mInitializeStrategy;
		private readonly IDatabaseInfo					mDatabaseInfo;
		private readonly IContextProvider				mContextProvider;

		public EntityFrameworkDatabaseManager( IDatabaseInitializeStrategy initializeStrategy, IDatabaseInfo databaseInfo, IContextProvider contextProvider ) {
			mInitializeStrategy = initializeStrategy;
			mDatabaseInfo = databaseInfo;
			mContextProvider = contextProvider;
		}

		public bool Initialize() {
			var retValue = false;

			if( mInitializeStrategy != null ) {
				retValue = mInitializeStrategy.InitializeDatabase( mContextProvider.CreateContext());

				if(( retValue ) &&
				   ( mInitializeStrategy.DidCreateDatabase )) {
					mDatabaseInfo.InitializeDatabaseVersion( cDatabaseVersionMajor, cDatabaseVersionMinor );
				}
			}

			mContextProvider.BlobStorageManager.Initialize( Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), Constants.CompanyName ));
			if(!mContextProvider.BlobStorageManager.IsOpen ) {
				if(!mContextProvider.BlobStorageManager.OpenStorage()) {
					mContextProvider.BlobStorageManager.CreateStorage();

					if(!mContextProvider.BlobStorageManager.OpenStorage()) {
						var ex = new ApplicationException( "EntityFrameworkDatabaseManager:Blob storage could not be created." );

						NoiseLogger.Current.LogException( ex );
						throw( ex );
					}
				}
			}

			return( retValue );
		}

		public void Shutdown() {
		}
	}
}
