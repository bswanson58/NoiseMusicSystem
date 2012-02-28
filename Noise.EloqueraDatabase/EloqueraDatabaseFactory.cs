using System;
using System.Globalization;
using System.IO;
using Noise.BlobStorage.BlobStore;
using Noise.EloqueraDatabase.Database;
using Noise.EloqueraDatabase.Interfaces;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase {
	public class EloqueraDatabaseFactory : IDatabaseFactory {
		private const string	cBlobStorageName	= "Noise Blobs";

		private readonly IIoc					mComponentCreator;
		private readonly IDatabaseInfo			mDatabaseInfo;
		private readonly DatabaseConfiguration	mDatabaseConfiguration;
		private readonly IBlobStorageManager	mBlobStorageManager;
		private bool							mBlobStorageInitialized;

		public EloqueraDatabaseFactory( IBlobStorageManager blobStorageManager, IIoc componentCreator,
										IDatabaseInfo databaseInfo, DatabaseConfiguration databaseConfiguration ) {
			mBlobStorageManager = blobStorageManager;
			mComponentCreator = componentCreator;
			mDatabaseInfo = databaseInfo;
			mDatabaseConfiguration = databaseConfiguration;

			mBlobStorageInitialized = false;
		}

		public IDatabase GetDatabaseInstance() {
			return( new EloqueraDb( mComponentCreator, mDatabaseConfiguration ));
		}

		public void SetBlobStorageInstance( IDatabase  database ) {
			if( database == null ) {
				throw new ApplicationException( "EloqueraDatabaseFactory:GetBlobStorageInstance passed null database." );
			}

			if( database.DatabaseVersion == null ) {
				throw new ApplicationException( "EloqueraDatabaseFactory:GetBlobStorageInstance passed uninitialized database instance." );
			}

			if(!mBlobStorageInitialized ) {
				InitBlobStorage();

				mBlobStorageInitialized = true;
			}

			database.BlobStorage = mBlobStorageManager.GetStorage();
		}

		private void InitBlobStorage() {
			mBlobStorageManager.Initialize( Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), Constants.CompanyName ));

			if(!mBlobStorageManager.IsOpen ) {
				var blobStorageName = Path.Combine( cBlobStorageName, mDatabaseInfo.DatabaseId.ToString( CultureInfo.InvariantCulture ));

				if(!mBlobStorageManager.OpenStorage( blobStorageName )) {
					mBlobStorageManager.CreateStorage( blobStorageName );

					if(!mBlobStorageManager.OpenStorage( blobStorageName )) {
						var ex = new ApplicationException( "EloqueraDatabaseFactory:Blob storage could not be created." );

						NoiseLogger.Current.LogException( ex );
						throw( ex );
					}
				}
			}
		}

		public void CloseFactory() {
			if( mBlobStorageManager != null ) {
				mBlobStorageManager.CloseStorage();
			}
		}
	}
}
