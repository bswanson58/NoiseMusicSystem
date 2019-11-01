using System;
using System.Collections.Generic;
using System.IO;

namespace ArchiveLoader.Interfaces {
    interface IDriveManager : IDisposable {
        IEnumerable<DriveInfo>  AvailableDrives { get; }

        string                  AddDriveNotification( DriveInfo forDrive, Action<DriveInfo> callback );
        void                    RemoveDriveNotification( string key );
    }
}
