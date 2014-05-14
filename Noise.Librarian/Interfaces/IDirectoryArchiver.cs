using System;
using Noise.Librarian.Models;

namespace Noise.Librarian.Interfaces {
	public interface IDirectoryArchiver {
		void	BackupDirectory( string sourcePath, string destinationName );
		void	BackupSubdirectories( string sourcePath, string destinationPath, Action<ProgressReport> progressCallback );

		void	RestoreDirectory( string sourceName, string destinationPath );
		void	RestoreSubdirectories( string sourcePath, string destinationPath );
	}
}
