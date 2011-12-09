using System;
using System.IO;
using Microsoft.Practices.Unity;
using Noise.EloqueraDatabase.BlobStore;
using Noise.EloqueraDatabase.Database;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase {
	public class EloqueraDatabaseFactory : IDatabaseFactory {
		private const string	cBlobStorageName	= "Noise Blobs";

		private readonly IBlobStorageResolver	mBlobResolver;
		private readonly IUnityContainer		mContainer;
		private IBlobStorageManager				mBlobStorageManager;

		public EloqueraDatabaseFactory( IBlobStorageResolver blobResolver, IUnityContainer container ) {
			mBlobResolver = blobResolver;
			mContainer = container;
		}

		public IDatabase GetDatabaseInstance() {
			return( new EloqueraDb( mContainer ));
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
				var blobStorageName = Path.Combine( cBlobStorageName, database.DatabaseVersion.DatabaseId.ToString());

				mBlobStorageManager = new BlobStorageManager( mBlobResolver, blobStoragePath );
					
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
