using System.Collections.Generic;
using System.IO;
using ArchiveLoader.Interfaces;
using ArchiveLoader.Platform;

namespace ArchiveLoader.Models {
    class DriveManager : IDriveManager {
        private readonly IDriveNotifier mDriveNotifier;

        public  IEnumerable<DriveInfo>  AvailableDrives => mDriveNotifier.DriveList;

        public DriveManager( IDriveNotifier driveNotifier ) {
            mDriveNotifier = driveNotifier;

            mDriveNotifier.OpticalDiskArrived += OnOpticalDiskArrived;
            mDriveNotifier.Start();
        }

        private void OnOpticalDiskArrived( object sender, OpticalDiskArrivedEventArgs opticalDiskArrivedEventArgs ) {
        }

        public void Dispose() {
            mDriveNotifier?.Dispose();
        }
    }
}

