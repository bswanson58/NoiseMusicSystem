using System;
using System.IO;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.EntityFrameworkDatabase.Logging;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DatabaseManager {
	internal class ContextProvider : IContextProvider {
		public	const string					cInvalidContextName = "_invalid_context_";

		private readonly ILogDatabase			mLog;
        private readonly IBlobStorageProvider   mBlobStorageProvider;
		private readonly ILibraryConfiguration	mLibraryConfiguration;

        public IBlobStorage                     BlobStorage => mBlobStorageProvider.BlobStorage;

		public ContextProvider( ILibraryConfiguration libraryConfiguration, ILogDatabase log, IBlobStorageProvider blobStorageProvider ) {
			mLog = log;
			mLibraryConfiguration = libraryConfiguration;
            mBlobStorageProvider = blobStorageProvider;
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
					connectionString = $@"Data Source=(localdb)\v11.0;Integrated Security=true;MultipleActiveResultSets=True;AttachDbFileName={databasePath}";

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
