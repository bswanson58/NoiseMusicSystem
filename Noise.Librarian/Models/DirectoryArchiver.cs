using System.IO.Compression;
using Noise.Librarian.Interfaces;

namespace Noise.Librarian.Models {
	public class DirectoryArchiver : IDirectoryArchiver {
		public void BackupDirectory( string sourcePath, string destinationName ) {
			ZipFile.CreateFromDirectory( sourcePath, destinationName, CompressionLevel.Optimal, false );
		}

		public void RestoreDirectory( string sourceName, string destinationPath ) {
			ZipFile.ExtractToDirectory( sourceName, destinationPath );
		}
	}
}
