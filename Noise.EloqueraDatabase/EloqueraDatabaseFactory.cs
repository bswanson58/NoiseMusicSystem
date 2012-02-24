using System;
using System.Globalization;
using System.IO;
using Noise.EloqueraDatabase.BlobStore;
using Noise.EloqueraDatabase.Database;
using Noise.EloqueraDatabase.Interfaces;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase {
	public class EloqueraDatabaseFactory : IDatabaseFactory {
		private const string	cBlobStorageName	= "Noise Blobs";

		private readonly IBlobStorageResolver	mBlobResolver;
		private readonly IIoc					mComponentCreator;
		private readonly DatabaseConfiguration	mDatabaseConfiguration;
		private IBlobStorageManager				mBlobStorageManager;

		public EloqueraDatabaseFactory( IBlobStorageResolver blobResolver, IIoc componentCreator, DatabaseConfiguration databaseConfiguration ) {
			mBlobResolver = blobResolver;
			mComponentCreator = componentCreator;
			mDatabaseConfiguration = databaseConfiguration;
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

			if( mBlobStorageManager == null ) {
				var blobStoragePath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), Constants.CompanyName );

				mBlobStorageManager = new BlobStorageManager( mBlobResolver, blobStoragePath );
			}

			if(!mBlobStorageManager.IsOpen ) {
				var blobStorageName = Path.Combine( cBlobStorageName, database.DatabaseVersion.DatabaseId.ToString( CultureInfo.InvariantCulture ));

				if(!mBlobStorageManager.OpenStorage( blobStorageName )) {
					mBlobStorageManager.CreateStorage( blobStorageName );

					if(!mBlobStorageManager.OpenStorage( blobStorageName )) {
						var ex = new ApplicationException( "EloqueraDatabaseFactory:Blob storage could not be created." );

						NoiseLogger.Current.LogException( ex );
						throw( ex );
					}
				}
	
			}

			database.BlobStorage = mBlobStorageManager.GetStorage();
		}

		public void CloseFactory() {
			if( mBlobStorageManager != null ) {
				mBlobStorageManager.CloseStorage();
			}
		}
	}
}
