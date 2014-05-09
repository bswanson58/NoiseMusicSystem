using System.Collections.Generic;
using System.Threading.Tasks;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface ILibraryConfiguration {
		LibraryConfiguration				Current { get; }
		IEnumerable<LibraryConfiguration>	Libraries { get; }

		void						Open( long libraryId );
		void						Open( LibraryConfiguration configuration );
		Task						AsyncOpen( LibraryConfiguration configuration );
		void						OpenDefaultLibrary();

		void						Close( LibraryConfiguration configuration );
		void						AddLibrary( LibraryConfiguration configuration );
		void						UpdateLibrary( LibraryConfiguration configuration );
		void						DeleteLibrary( LibraryConfiguration configuration );

		string						GetLibraryFolder( LibraryConfiguration libraryConfiguration );

		IEnumerable<LibraryBackup>	GetLibraryBackups( LibraryConfiguration forLibrary );
		LibraryBackup				OpenLibraryBackup( LibraryConfiguration libraryConfiguration );
		void						CloseLibraryBackup( LibraryConfiguration libraryConfiguration, LibraryBackup backup );
		void						AbortLibraryBackup( LibraryConfiguration libraryConfiguration, LibraryBackup backup );

		LibraryConfiguration		OpenLibraryRestore( LibraryConfiguration library, LibraryBackup fromBackup );
		void						CloseLibraryRestore( LibraryConfiguration library );
	}
}
