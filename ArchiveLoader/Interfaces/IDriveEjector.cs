using System.Threading.Tasks;

namespace ArchiveLoader.Interfaces {
    interface IDriveEjector {
        Task    OpenDrive( char driveLetter );
        Task    CloseDrive( char driveLetter );
    }
}
