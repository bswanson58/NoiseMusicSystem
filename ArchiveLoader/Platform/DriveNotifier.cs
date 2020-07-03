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
        private readonly IPlatformLog   mLog;
        private readonly Dictionary<string, bool>      mDriveState;
        private Timer                   mDriveTimer;
        private bool                    mHaveDisk;

        /// <summary>
        ///     Gets or sets the time, in seconds, before the drive watcher checks for new media insertion relative to the last occurance of check.
        /// </summary>
        public  int                     Interval = 1;
        public  IEnumerable<string>     DriveList => mDriveState.Keys;

        /// <summary>
        ///     Occurs when a new optical disk is inserted or ejected.
        /// </summary>
        public event OpticalDiskArrivedEventHandler OpticalDiskArrived;

        public DriveNotifier( IPlatformLog log ) {
            mLog = log;

            mDriveState = new Dictionary<string, bool>();
        }

        private void OnOpticalDiskArrived( OpticalDiskArrivedEventArgs e ) {
            OpticalDiskArrived?.Invoke( this, e );
        }

        public void Start() {
            mDriveState.Clear();

            foreach ( var drive in DriveInfo.GetDrives().Where( driveInfo => driveInfo.DriveType.Equals( DriveType.CDRom ))) {
                mDriveState.Add( drive.Name, drive.IsReady );

//                mLog.LogMessage( $"DriveNotifier: Drive: '{drive.Name}' IsReady = {drive.IsReady}" );
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
                                          where mDriveState.ContainsKey( drive.Name )
                                          select drive ) {
                        if(!mDriveState[drive.Name].Equals( drive.IsReady )) {
                            mDriveState[drive.Name] = drive.IsReady;

                            OnOpticalDiskArrived(new OpticalDiskArrivedEventArgs { Drive = drive });

//                            mLog.LogMessage($"DriveNotifier: Drive: '{drive.Name}' IsReady = {drive.IsReady}");
                        }
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
