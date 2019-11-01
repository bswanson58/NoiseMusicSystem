using System;
using System.Collections.Generic;
using System.IO;

namespace ArchiveLoader.Interfaces {
    interface IDriveManager : IDisposable {
        IEnumerable<DriveInfo>  AvailableDrives { get; }
    }
}
