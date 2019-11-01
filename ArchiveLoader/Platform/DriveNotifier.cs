using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;
using ArchiveLoader.Interfaces;

namespace ArchiveLoader.Platform {
    public class OpticalDiskArrivedEventArgs : EventArgs {
        public DriveInfo Drive { get; set; }
    }

    public class DriveNotifier : IDriveNotifier {
        private readonly Dictionary<string, DriveInfo> mDrives;
        private readonly Dictionary<string, bool>      mDriveState;
        private Timer                   mDriveTimer;
        private bool                    mHaveDisk;

        /// <summary>
        ///     Gets or sets the time, in seconds, before the drive watcher checks for new media insertion relative to the last occurance of check.
        /// </summary>
        public  int                     Interval = 1;
        public  IEnumerable<DriveInfo>  DriveList => mDrives.Values;

        /// <summary>
        ///     Occurs when a new optical disk is inserted or ejected.
        /// </summary>
        public event OpticalDiskArrivedEventHandler OpticalDiskArrived;

        public DriveNotifier() {
            mDrives = new Dictionary<string, DriveInfo>();
            mDriveState = new Dictionary<string, bool>();
        }

        private void OnOpticalDiskArrived( OpticalDiskArrivedEventArgs e ) {
            var handler = OpticalDiskArrived;

            handler?.Invoke( this, e );
        }

        public void Start() {
            mDrives.Clear();

            foreach( var drive in DriveInfo.GetDrives().Where( driveInfo => driveInfo.DriveType.Equals( DriveType.CDRom ))) {
                mDrives.Add( drive.Name, drive );
                mDriveState.Add( drive.Name, drive.IsReady );
            }

            mDriveTimer = new Timer { Interval = Interval * 1000 };
            mDriveTimer.Elapsed += DriveTimerOnElapsed;
            mDriveTimer.Start();
        }

        public void Stop() {
            if( mDriveTimer != null ) {
                mDriveTimer.Stop();
                mDriveTimer.Dispose();
            }
        }

        private void DriveTimerOnElapsed( object sender, ElapsedEventArgs elapsedEventArgs ) {
            if(!mHaveDisk ) {
                try {
                    mHaveDisk = true;
                    foreach( var drive in from drive in DriveInfo.GetDrives()
                                          where drive.DriveType.Equals( DriveType.CDRom )
                                          where mDrives.ContainsKey( drive.Name )
                                          where !mDriveState[drive.Name].Equals( drive.IsReady )
                                          select drive ) {
                        mDriveState[drive.Name] = drive.IsReady;

                        OnOpticalDiskArrived( new OpticalDiskArrivedEventArgs { Drive = drive });
                    }
                }
                catch( Exception exception ) {
                    Debug.Write( exception.Message );
                }
                finally {
                    mHaveDisk = false;
                }
            }
        }

        public void Dispose() {
            mDriveTimer?.Dispose();
        }
    }
}
