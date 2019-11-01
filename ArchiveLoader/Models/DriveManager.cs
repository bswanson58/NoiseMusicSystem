using System;
using System.Collections.Generic;
using System.IO;
using ArchiveLoader.Interfaces;
using ArchiveLoader.Platform;

namespace ArchiveLoader.Models {
    internal class DriveNotification {
        public string               Key { get; }
        public DriveInfo            Drive { get; }
        public Action<DriveInfo>    Callback { get; }

        public DriveNotification( DriveInfo forDrive, Action<DriveInfo> callback ) {
            Key = new Guid().ToString();
            Drive = forDrive;
            Callback = callback;
        }
    }

    class DriveManager : IDriveManager {
        private readonly IDriveNotifier             mDriveNotifier;
        private readonly List<DriveNotification>    mNotificationList;

        public  IEnumerable<DriveInfo>  AvailableDrives => mDriveNotifier.DriveList;

        public DriveManager( IDriveNotifier driveNotifier ) {
            mDriveNotifier = driveNotifier;

            mNotificationList = new List<DriveNotification>();

            mDriveNotifier.OpticalDiskArrived += OnOpticalDiskArrived;
            mDriveNotifier.Start();
        }

        public string AddDriveNotification( DriveInfo forDrive, Action<DriveInfo> callback ) {
            var notification = new DriveNotification(forDrive, callback);
            var retValue = notification.Key;

            lock ( mNotificationList ) {
                mNotificationList.Add( notification );
            }

            return retValue;
        }

        public void RemoveDriveNotification( string key ) {
            lock( mNotificationList ) {
                mNotificationList.RemoveAll( n => n.Key.Equals( key ));
            }
        }

        private void OnOpticalDiskArrived( object sender, OpticalDiskArrivedEventArgs opticalDiskArrivedEventArgs ) {
            var notificationList = new List<DriveNotification>();

            lock( mNotificationList ) {
                mNotificationList.ForEach( i => {
                    if( i.Drive.Name.Equals( opticalDiskArrivedEventArgs.Drive.Name )) {
                        notificationList.Add( i );
                    }
                });
            }

            notificationList.ForEach( n => n.Callback( n.Drive ));
        }

        public void Dispose() {
            mDriveNotifier?.Dispose();
        }
    }
}

