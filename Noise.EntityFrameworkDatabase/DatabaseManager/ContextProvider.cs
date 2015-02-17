using System;
using System.IO;
using Noise.BlobStorage.BlobStore;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DatabaseManager {
	public class ContextProvider : IContextProvider {
		public	const string					cInvalidContextName = "_invalid_context_";

		private readonly INoiseLog				mLog;
		private readonly IBlobStorageManager	mBlobStorageManager;
		private readonly ILibraryConfiguration	mLibraryConfiguration;

		public ContextProvider( ILibraryConfiguration libraryConfiguration, INoiseLog log,
								IBlobStorageManager blobStorageManager, IBlobStorageResolver storageResolver ) {
			mLog = log;
			mLibraryConfiguration = libraryConfiguration;
			mBlobStorageManager = blobStorageManager;
			mBlobStorageManager.SetResolver( storageResolver );
		}

		public IBlobStorageManager BlobStorageManager {
			get{ return( mBlobStorageManager ); }
		}

		public IDbContext	CreateContext() {
			var databaseName = cInvalidContextName;
			var connectionString = string.Empty;

			if( mLibraryConfiguration.Current != null ) {
				try {
					var databasePath = mLibraryConfiguration.Current.LibraryDatabasePath;

					if( !Directory.Exists( databasePath )) {
						Directory.CreateDirectory( databasePath );
					}

					databaseName = mLibraryConfiguration.Current.DatabaseName;
					databasePath = Path.Combine( databasePath, FormatDatabaseName( mLibraryConfiguration.Current.DatabaseName  ));
					connectionString = string.Format( @"Data Source=(localdb)\v11.0;Integrated Security=true;MultipleActiveResultSets=True;AttachDbFileName={0}", databasePath );

				}
				catch( Exception ex ) {
					mLog.LogException( "Creating database context", ex );
				}
			}

			return( new NoiseContext( databaseName, connectionString ));
		}

		public static string FormatDatabaseName( string libraryName ) {
			return( libraryName + Constants.Ef_DatabaseFileExtension );
		}
	}
}
