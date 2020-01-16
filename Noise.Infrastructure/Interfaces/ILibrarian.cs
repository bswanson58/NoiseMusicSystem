using System;
using System.Threading.Tasks;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface ILibrarian {
        Task	BackupLibraryAsync( LibraryConfiguration configuration, Action<LibrarianProgressReport> progressCallback );
        void	BackupLibrary( LibraryConfiguration library, Action<LibrarianProgressReport> progressCallback );
		void	RestoreLibrary( LibraryConfiguration library, LibraryBackup libraryBackup, Action<LibrarianProgressReport> progressCallback );

		void	ExportLibrary( LibraryConfiguration library, string exportPath, Action<LibrarianProgressReport> progressCallback );
		void	ImportLibrary( LibraryConfiguration library, LibraryBackup libraryBackup, Action<LibrarianProgressReport> progressCallback );
	}
}
