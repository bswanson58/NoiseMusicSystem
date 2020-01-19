using System;
using System.Threading.Tasks;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
    public interface ILibraryBackupManager {
        Task<bool>  BackupLibrary( Action<LibrarianProgressReport> progress );
    }
}
