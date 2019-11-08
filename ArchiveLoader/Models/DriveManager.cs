using System;
using System.Collections.Generic;
using System.IO;
using ArchiveLoader.Interfaces;
using ArchiveLoader.Platform;

namespace ArchiveLoader.Models {
    internal class DriveNotification {
        public string               Key { get; }
        public string               Drive { get; }
        public Action<DriveInfo>    Callback { get; }

        public DriveNotification( string forDrive, Action<DriveInfo> callback ) {
            Key = Guid.NewGuid().ToString();
            Drive = forDrive;
            Callback = callback;
        }
    }

    class DriveManager : IDriveManager {
        private readonly IPlatformLog               mLog;
        private readonly IDriveNotifier             mDriveNotifier;
        private readonly List<DriveNotification>    mNotificationList;

        public  IEnumerable<string>     AvailableDrives => mDriveNotifier.DriveList;

        public DriveManager( IDriveNotifier driveNotifier, IPlatformLog log ) {
            mLog = log;
            mDriveNotifier = driveNotifier;

            mNotificationList = new List<DriveNotification>();

            mDriveNotifier.OpticalDiskArrived += OnOpticalDiskArrived;
            mDriveNotifier.Start();
        }

        public string AddDriveNotification( string driveName, Action<DriveInfo> callback ) {
            var notification = new DriveNotification( driveName, callback );
            var retValue = notification.Key;

            lock( mNotificationList ) {
                mNotificationList.Add( notification );
            }

//            mLog.LogMessage( $"DriveManager: Waiting for drive {driveName} to be ready." );

            return retValue;
        }

        public void RemoveDriveNotification( string key ) {
            lock( mNotificationList ) {
                mNotificationList.RemoveAll( n => n.Key.Equals( key ));
            }
        }

        private void OnOpticalDiskArrived( object sender, OpticalDiskArrivedEventArgs opticalDiskArrivedEventArgs ) {
            var notificationList = new List<Action>();

            lock( mNotificationList ) {
                mNotificationList.ForEach( i => {
                    if( i.Drive.Equals( opticalDiskArrivedEventArgs.Drive.Name )) {
                        notificationList.Add( () => { i.Callback?.Invoke( opticalDiskArrivedEventArgs.Drive );});
                    }
                });
            }

            notificationList.ForEach( callback => { callback(); });
        }

        public void Dispose() {
            mDriveNotifier?.Dispose();
        }
    }
}

