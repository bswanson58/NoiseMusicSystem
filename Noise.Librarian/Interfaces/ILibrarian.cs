using System;
using Noise.Infrastructure.Dto;
using Noise.Librarian.Models;

namespace Noise.Librarian.Interfaces {
	public interface ILibrarian {
		bool	Initialize();
		void	Shutdown();

		void	BackupLibrary( LibraryConfiguration library, Action<ProgressReport> progressCallback );
		void	RestoreLibrary( LibraryConfiguration library, LibraryBackup libraryBackup, Action<ProgressReport> progressCallback );

		void	ExportLibrary( LibraryConfiguration library, string exportPath, Action<ProgressReport> progressCallback );
		void	ImportLibrary( LibraryConfiguration library, LibraryBackup libraryBackup, Action<ProgressReport> progressCallback );
	}
}
