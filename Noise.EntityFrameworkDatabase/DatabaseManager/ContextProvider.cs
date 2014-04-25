using System;
using System.IO;
using Noise.BlobStorage.BlobStore;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DatabaseManager {
	public class ContextProvider : IContextProvider {
		public	const string					cInvalidContextName = "_invalid_context_";

		private readonly IBlobStorageManager	mBlobStorageManager;
		private readonly ILibraryConfiguration	mLibraryConfiguration;

		public ContextProvider( ILibraryConfiguration libraryConfiguration,
								IBlobStorageManager blobStorageManager, IBlobStorageResolver storageResolver ) {
			mLibraryConfiguration = libraryConfiguration;
			mBlobStorageManager = blobStorageManager;
			mBlobStorageManager.SetResolver( storageResolver );
		}

		public IBlobStorageManager BlobStorageManager {
			get{ return( mBlobStorageManager ); }
		}

		public IDbContext	CreateContext() {
			var databaseName = cInvalidContextName;

			if( mLibraryConfiguration.Current != null ) {
				try {
					var databasePath = mLibraryConfiguration.Current.LibraryDatabasePath;

					if( !Directory.Exists( databasePath ) ) {
						Directory.CreateDirectory( databasePath );
					}

					databaseName = Path.Combine( databasePath, mLibraryConfiguration.Current.DatabaseName );
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( "ContextProvider:CreateContext", ex );
				}
			}

			return( new NoiseContext( databaseName ));
		}
	}
}
