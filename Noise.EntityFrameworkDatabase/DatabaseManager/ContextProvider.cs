using System;
using System.Globalization;
using System.IO;
using Noise.BlobStorage.BlobStore;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DatabaseManager {
	public class ContextProvider : IContextProvider {
		private readonly IBlobStorageManager	mBlobStorageManager;
		private readonly ILibraryConfiguration	mLibraryConfiguration;

		public ContextProvider( ILibraryConfiguration libraryConfiguration, IBlobStorageManager blobStorageManager ) {
			mLibraryConfiguration = libraryConfiguration;
			mBlobStorageManager = blobStorageManager;
		}

		public IBlobStorageManager BlobStorageManager {
			get{ return( mBlobStorageManager ); }
		}

		public IDbContext	CreateContext() {
			var databaseName = "unknown";

			if( mLibraryConfiguration.Current != null ) {
				try {
					var databasePath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.CommonApplicationData ),
													Constants.CompanyName, 
													Constants.LibraryConfigurationDirectory,
													mLibraryConfiguration.Current.LibraryId.ToString( CultureInfo.InvariantCulture ));

					Directory.CreateDirectory( databasePath );

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
