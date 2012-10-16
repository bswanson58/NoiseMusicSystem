using System;
using Noise.BlobStorage.BlobStore;
using Noise.EloqueraDatabase.Database;
using Noise.EloqueraDatabase.Interfaces;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase {
	public class EloqueraDatabaseFactory : IDatabaseFactory {
		private readonly IIoc					mComponentCreator;
		private readonly ILibraryConfiguration	mLibraryConfiguration;
		private readonly IBlobStorageManager	mBlobStorageManager;
		private bool							mBlobStorageInitialized;

		public EloqueraDatabaseFactory( IBlobStorageManager blobStorageManager, IBlobStorageResolver storageResolver,
										IIoc componentCreator, ILibraryConfiguration libraryConfiguration ) {
			mBlobStorageManager = blobStorageManager;
			mBlobStorageManager.SetResolver( storageResolver );
			mComponentCreator = componentCreator;
			mLibraryConfiguration = libraryConfiguration;

			mBlobStorageInitialized = false;
		}

		public IDatabase GetDatabaseInstance() {
			return( new EloqueraDb( mComponentCreator, mLibraryConfiguration.Current ));
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
			mBlobStorageManager.Initialize( mLibraryConfiguration.Current.BlobDatabasePath );

			if(!mBlobStorageManager.IsOpen ) {
				if(!mBlobStorageManager.OpenStorage()) {
					mBlobStorageManager.CreateStorage();

					if(!mBlobStorageManager.OpenStorage()) {
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
