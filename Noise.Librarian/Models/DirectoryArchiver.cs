using System.IO;
using System.IO.Compression;
using Noise.Librarian.Interfaces;

namespace Noise.Librarian.Models {
	public class DirectoryArchiver : IDirectoryArchiver {
		public void BackupDirectory( string sourcePath, string destinationName ) {
			ZipFile.CreateFromDirectory( sourcePath, destinationName, CompressionLevel.Optimal, false );
		}

		public void RestoreDirectory( string sourceName, string destinationPath ) {
			Compression.ImprovedExtractToDirectory( sourceName, destinationPath, Compression.Overwrite.Always );
		}

		public void BackupSubdirectories( string sourcePath, string destinationPath ) {
			if( Directory.Exists( sourcePath )) {
				var directoryList = Directory.EnumerateDirectories( sourcePath );

				foreach( var directory in directoryList ) {
					var archiveName = Path.Combine( destinationPath, Path.GetFileName( directory ));

					ZipFile.CreateFromDirectory( directory, archiveName, CompressionLevel.Fastest, true );
				}
			}
		}

		public void RestoreSubdirectories( string sourcePath, string destinationPath ) {
			if( Directory.Exists( sourcePath )) {
				var archiveList = Directory.EnumerateFiles( sourcePath );

				foreach( var archiveFile in archiveList ) {
					Compression.ImprovedExtractToDirectory( archiveFile, destinationPath, Compression.Overwrite.Always );
				}
			}
		}
	}
}
