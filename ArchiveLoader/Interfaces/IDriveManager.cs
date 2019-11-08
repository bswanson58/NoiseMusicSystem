using System;
using System.Collections.Generic;
using System.IO;

namespace ArchiveLoader.Interfaces {
    interface IDriveManager : IDisposable {
        IEnumerable<string>     AvailableDrives { get; }

        string                  AddDriveNotification( string forDrive, Action<DriveInfo> callback );
        void                    RemoveDriveNotification( string key );
    }
}
