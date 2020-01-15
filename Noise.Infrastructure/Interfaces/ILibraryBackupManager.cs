using System;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
    public interface ILibraryBackupManager {
        void    BackupLibrary( Action<LibrarianProgressReport> progress );
    }
}
