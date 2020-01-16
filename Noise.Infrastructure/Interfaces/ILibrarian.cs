using System;
using System.Threading.Tasks;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface ILibrarian {
        Task	BackupLibrary( LibraryConfiguration configuration, Action<LibrarianProgressReport> progressCallback );
		Task	RestoreLibrary( LibraryConfiguration library, LibraryBackup libraryBackup, Action<LibrarianProgressReport> progressCallback );

		Task	ExportLibrary( LibraryConfiguration library, string exportPath, Action<LibrarianProgressReport> progressCallback );
		Task	ImportLibrary( LibraryConfiguration library, LibraryBackup libraryBackup, Action<LibrarianProgressReport> progressCallback );
	}
}
