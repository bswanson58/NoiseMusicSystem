﻿using System.Collections.Generic;
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

		void						UpdateConfiguration( LibraryConfiguration configuration );

		string						GetLibraryFolder( LibraryConfiguration libraryConfiguration );

		IEnumerable<LibraryBackup>	GetLibraryBackups( LibraryConfiguration forLibrary );
		LibraryBackup				OpenLibraryBackup( LibraryConfiguration libraryConfiguration );
		void						CloseLibraryBackup( LibraryConfiguration libraryConfiguration, LibraryBackup backup );
		void						AbortLibraryBackup( LibraryConfiguration libraryConfiguration, LibraryBackup backup );

		void						OpenLibraryRestore( LibraryConfiguration libraryConfiguration, LibraryBackup backup );
		void						CloseLibraryRestore( LibraryConfiguration libraryConfiguration, LibraryBackup backup );
		void						AbortLibraryRestore( LibraryConfiguration libraryConfiguration, LibraryBackup backup );

		LibraryBackup				OpenLibraryExport( LibraryConfiguration libraryConfiguration, string exportPath );
		void						CloseLibraryExport( LibraryConfiguration libraryConfiguration, LibraryBackup backup );
		void						AbortLibraryExport( LibraryConfiguration libraryConfiguration, LibraryBackup backup );

		LibraryConfiguration		OpenLibraryImport( LibraryConfiguration library, LibraryBackup fromBackup );
		void						CloseLibraryImport( LibraryConfiguration library );
		void						AbortLibraryImport( LibraryConfiguration library );
	}
}
