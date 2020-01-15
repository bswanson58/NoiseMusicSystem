using System;
using System.Threading.Tasks;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
    public interface ILibraryBackupManager {
        Task    BackupLibrary( Action<LibrarianProgressReport> progress );
    }
}
