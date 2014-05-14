﻿using System;
using Noise.Infrastructure.Dto;
using Noise.Librarian.Models;

namespace Noise.Librarian.Interfaces {
	public interface ILibrarian {
		bool	Initialize();
		void	Shutdown();

		void	BackupLibrary( LibraryConfiguration library, Action<ProgressReport> progressCallback );
		void	RestoreLibrary( LibraryConfiguration library, LibraryBackup libraryBackup );

		void	ExportLibrary( LibraryConfiguration library, string exportPath );
		void	ImportLibrary( LibraryConfiguration library, LibraryBackup libraryBackup );
	}
}
