using System.Collections.Generic;
using System.Threading.Tasks;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface ILibraryConfiguration {
		LibraryConfiguration				Current { get; }
		IEnumerable<LibraryConfiguration>	Libraries { get; }

		void		Open( long libraryId );
		void		Open( LibraryConfiguration configuration );
		Task		AsyncOpen( LibraryConfiguration configuration );
		void		OpenDefaultLibrary();

		void		Close( LibraryConfiguration configuration );
		void		AddLibrary( LibraryConfiguration configuration );
		void		UpdateLibrary( LibraryConfiguration configuration );
		void		DeleteLibrary( LibraryConfiguration configuration );

		string		GetLibraryFolder( LibraryConfiguration libraryConfiguration );

		string		OpenLibraryBackup( LibraryConfiguration libraryConfiguration );
		void		CloseLibraryBackup( LibraryConfiguration libraryConfiguration, string backupDirectory );
		void		AbortLibraryBackup( LibraryConfiguration libraryConfiguration, string backupDirectory );
	}
}
