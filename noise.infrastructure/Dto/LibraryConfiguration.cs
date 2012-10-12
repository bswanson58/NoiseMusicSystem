using System;
using System.Collections.Generic;
using System.IO;
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

	public class LibraryConfiguration {
		private string				mConfigurationPath;
		public	long				LibraryId { get; set; }
		public	string				LibraryName { get; set; }
		public	string				DatabaseName { get; set; }
		public	string				DatabaseServer { get; set; }
		public	string				DatabaseUser { get; set; }
		public	string				DatabasePassword { get; set; }
		public	bool				IsDefaultLibrary { get; set; }
		public	List<MediaLocation>	MediaLocations { get; set; }

		public LibraryConfiguration() {
			LibraryId = DatabaseIdentityProvider.Current.NewIdentityAsLong();
			MediaLocations = new List<MediaLocation>();

			mConfigurationPath = string.Empty;
			LibraryName = string.Empty;
			DatabaseName = string.Empty;
			DatabaseServer = "localhost";
			DatabaseUser = string.Empty;
			DatabasePassword = string.Empty;
		}

		public static LibraryConfiguration LoadConfiguration( string fromPath ) {
			LibraryConfiguration	retValue = null;

			try {
				var stream = new FileStream( fromPath, FileMode.Open, FileAccess.Read );
				var serializer = new DataContractJsonSerializer( typeof( LibraryConfiguration ));

				retValue= serializer.ReadObject( stream ) as LibraryConfiguration;
				stream.Close();

				if( retValue != null ) {
					retValue.SetConfigurationPath( Path.GetDirectoryName( fromPath ));
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "LibraryConfiguration:Load", ex );
			}

			return( retValue );
		}

		public void SetConfigurationPath( string path ) {
			mConfigurationPath = path;
		}

		public void Persist( string toPath ) {
			try {
				var stream = new FileStream( toPath, FileMode.Create, FileAccess.Write );
				var serializer = new DataContractJsonSerializer( GetType());

				serializer.WriteObject( stream, this );
				stream.Close();

				SetConfigurationPath( Path.GetDirectoryName( toPath ));
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "LibraryConfiguration:Persist", ex );
			}
		}

		public string BlobDatabasePath {
			get{ return( Path.Combine( mConfigurationPath, Constants.BlobDatabaseDirectory )); }
		}

		public string SearchDatabasePath {
			get{ return( Path.Combine( mConfigurationPath, Constants.SearchDatabaseDirectory )); }
		}
	}
}
