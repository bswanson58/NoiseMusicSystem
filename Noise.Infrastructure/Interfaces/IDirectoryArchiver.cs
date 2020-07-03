using System;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IDirectoryArchiver {
		void	BackupDirectory( string sourcePath, string destinationName );
		void	BackupSubdirectories( string sourcePath, string destinationPath, Action<LibrarianProgressReport> progressCallback );

		void	RestoreDirectory( string sourceName, string destinationPath );
		void	RestoreSubdirectories( string sourcePath, string destinationPath, Action<LibrarianProgressReport> progressCallback );
	}
}
