using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using Noise.Infrastructure.Support;

namespace Noise.Infrastructure.Dto {
	public class MediaLocation {
		public	long					Key { get; set; }
		public	string					Path { get; set; }
		public	bool					PreferFolderStrategy { get; set; }
		public	List<eFolderStrategy>	FolderStrategy { get; set; }

		public MediaLocation() {
			Key = DatabaseIdentityProvider.Current.NewIdentityAsLong();

			FolderStrategy = new List<eFolderStrategy>( new [] { eFolderStrategy.Undefined,
																 eFolderStrategy.Undefined, 
																 eFolderStrategy.Undefined, 
																 eFolderStrategy.Undefined, 
																 eFolderStrategy.Undefined } );
			Path = string.Empty;
		}
	}

    [DebuggerDisplay("Library = {" + nameof(LibraryName) + "}")]
	public class LibraryConfiguration {
		private string				mConfigurationPath;
		public	long				LibraryId { get; set; }
		public	string				LibraryName { get; set; }
        public  string              BlobDatabaseLocation { get; set; }
		public	string				DatabaseName { get; set; }
		public	string				DatabaseServer { get; set; }
		public	string				DatabaseUser { get; set; }
		public	string				DatabasePassword { get; set; }
		public	bool				IsDefaultLibrary { get; set; }
        public  bool                IsMetadataInPlace { get; set; }
		public	uint				BackupPressure { get; set; }
		public	List<MediaLocation>	MediaLocations { get; set; }

        public  string              BlobDatabasePath => !String.IsNullOrWhiteSpace( BlobDatabaseLocation ) ? BlobDatabaseLocation : 
                                                                                                            Path.Combine( mConfigurationPath, Constants.BlobDatabaseDirectory );
        public  string              LibraryDatabasePath => Path.Combine( mConfigurationPath, Constants.LibraryDatabaseDirectory );
        public  string              SearchDatabasePath => Path.Combine( mConfigurationPath, Constants.SearchDatabaseDirectory );

		public LibraryConfiguration() {
			LibraryId = DatabaseIdentityProvider.Current.NewIdentityAsLong();
			MediaLocations = new List<MediaLocation>();

			mConfigurationPath = string.Empty;
			LibraryName = string.Empty;
			DatabaseName = string.Empty;
			DatabaseServer = "localhost";
			DatabaseUser = string.Empty;
			DatabasePassword = string.Empty;
            BlobDatabaseLocation = string.Empty;
            IsMetadataInPlace = true;
			BackupPressure = 0;
		}

		public static LibraryConfiguration LoadConfiguration( string fromPath ) {
			var stream = new FileStream( fromPath, FileMode.Open, FileAccess.Read );
			var serializer = new DataContractJsonSerializer( typeof( LibraryConfiguration ));
			var retValue = serializer.ReadObject( stream ) as LibraryConfiguration;
			stream.Close();

            retValue?.SetConfigurationPath( Path.GetDirectoryName( fromPath ));

            return( retValue );
		}

		public void SetConfigurationPath( string path ) {
			mConfigurationPath = path;
		}

		public void Persist( string toPath ) {
			var stream = new FileStream( toPath, FileMode.Create, FileAccess.Write );
			var serializer = new DataContractJsonSerializer( GetType());

			serializer.WriteObject( stream, this );
			stream.Close();

			// this would change the configuration path to a backup location during a database backup...
//			SetConfigurationPath( Path.GetDirectoryName( toPath ));
		}

        public override string ToString() {
			var mediaPath = MediaLocations.Any() ? MediaLocations[0].Path : string.Empty;

			return( $"Library \"{LibraryName}\", Database \"{DatabaseName}\", Media \"{mediaPath}\"" );
		}
	}
}
