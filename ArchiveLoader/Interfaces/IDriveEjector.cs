using System.Threading.Tasks;

namespace ArchiveLoader.Interfaces {
    interface IDriveEjector {
        Task    OpenDrive( string driveRoot );
        Task    CloseDrive( string driveRoot );
    }
}
