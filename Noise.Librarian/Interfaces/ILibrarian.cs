using Noise.Infrastructure.Dto;

namespace Noise.Librarian.Interfaces {
	public interface ILibrarian {
		bool	Initialize();
		void	Shutdown();

		void	BackupLibrary( LibraryConfiguration library );
		void	RestoreLibrary( LibraryConfiguration library, LibraryBackup libraryBackup );

		void	ExportLibrary( LibraryConfiguration library, string exportPath );
		void	ImportLibrary( LibraryConfiguration library, LibraryBackup libraryBackup );
	}
}
