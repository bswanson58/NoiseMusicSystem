using System;
using System.Threading.Tasks;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface ILibrarian {
        Task<bool>	BackupLibrary( LibraryConfiguration configuration, Action<LibrarianProgressReport> progressCallback );
		Task<bool>	RestoreLibrary( LibraryConfiguration library, LibraryBackup libraryBackup, Action<LibrarianProgressReport> progressCallback );

		Task<bool>	ExportLibrary( LibraryConfiguration library, string exportPath, Action<LibrarianProgressReport> progressCallback );
		Task<bool>	ImportLibrary( LibraryConfiguration library, LibraryBackup libraryBackup, Action<LibrarianProgressReport> progressCallback );
	}
}
