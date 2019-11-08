using System;
using System.Collections.Generic;
using ArchiveLoader.Platform;

namespace ArchiveLoader.Interfaces {
    public delegate void OpticalDiskArrivedEventHandler( Object sender, OpticalDiskArrivedEventArgs e );

    public interface IDriveNotifier : IDisposable {
        event OpticalDiskArrivedEventHandler OpticalDiskArrived;

        void                Start();
        void                Stop();

        IEnumerable<string> DriveList { get; }
    }
}
