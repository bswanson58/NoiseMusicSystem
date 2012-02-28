using System;
using System.Globalization;
using System.IO;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DatabaseManager {
	public class EntityFrameworkDatabaseManager : IDatabaseManager {
		private const string	cBlobStorageName	= "Noise Blobs";

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
			}

			mContextProvider.BlobStorageManager.Initialize( Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), Constants.CompanyName ));
			if(!mContextProvider.BlobStorageManager.IsOpen ) {
				var blobStorageName = Path.Combine( cBlobStorageName, mDatabaseInfo.DatabaseId.ToString( CultureInfo.InvariantCulture ));

				if(!mContextProvider.BlobStorageManager.OpenStorage( blobStorageName )) {
					mContextProvider.BlobStorageManager.CreateStorage( blobStorageName );

					if(!mContextProvider.BlobStorageManager.OpenStorage( blobStorageName )) {
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
