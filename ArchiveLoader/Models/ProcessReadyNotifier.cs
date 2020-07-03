using System;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using ArchiveLoader.Dto;
using ArchiveLoader.Interfaces;

namespace ArchiveLoader.Models {
    class ProcessReadyNotifier : IProcessReadyNotifier {
        private readonly IDriveEjector              mDriveEjector;
        private readonly IDriveManager              mDriveManager;
        private readonly IPreferences               mPreferences;
        private readonly IPlatformLog               mLog;
        private readonly Subject<Events.JobTargets> mJobSubject;
        private string                              mSourceDirectory;
        private string                              mTargetDirectory;
        private string                              mSourceVolumeName;
        private string                              mDriveNotificationKey;
        private IDisposable                         mProcessQueueSubscription;

        public IObservable<Events.JobTargets>       OnJobReady => mJobSubject;

        public ProcessReadyNotifier( IDriveManager driveManager, IDriveEjector driveEjector, IPreferences preferences, IPlatformLog log ) {
            mDriveManager = driveManager;
            mDriveEjector = driveEjector;
            mPreferences = preferences;
            mLog = log;

            mDriveNotificationKey = String.Empty;
            mJobSubject = new Subject<Events.JobTargets>();
        }

        public void StartNotifying() {
            StopNotifying();

            var preferences = mPreferences.Load<ArchiveLoaderPreferences>();

            mTargetDirectory = preferences.TargetDirectory;

            if((!String.IsNullOrWhiteSpace( preferences.SourceDirectory )) &&
               ( Directory.Exists(preferences.SourceDirectory ))) {
                mSourceDirectory = preferences.SourceDirectory;
                mSourceVolumeName = preferences.SourceDirectory;

                OnSourceAvailable();
            }
            else {
                var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.Name.Equals( preferences.SourceDrive ));

                if(( drive != null ) &&
                   (!String.IsNullOrWhiteSpace( mTargetDirectory )) &&
                   ( Directory.Exists(mTargetDirectory ))) {
                    mSourceDirectory = drive.Name;

                    mDriveEjector.OpenDrive( drive.Name );

                    mDriveNotificationKey = mDriveManager.AddDriveNotification( mSourceDirectory, OnDriveNotification );
                }
                else {
                    mLog.LogMessage( "ProcessManager: Cannot start processing." );
                }
            }
        }

        public void StopNotifying() {
            if(!String.IsNullOrWhiteSpace( mDriveNotificationKey )) {
                mDriveManager.RemoveDriveNotification( mDriveNotificationKey );

                mDriveNotificationKey = String.Empty;
            }
        }

        public void JobCompleted( string sourceDrive ) {
            var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.Name.Equals( sourceDrive ));

            if( drive != null ) {
                mDriveEjector.OpenDrive( drive.Name );

                mLog.LogMessage( $"Ejected drive {drive.Name}" );
            }
        }

        private void OnDriveNotification( DriveInfo drive ) {
            mLog.LogMessage( $"Drive {drive.VolumeLabel} IsReady = {drive.IsReady}" );

            if(( drive.Name.Equals( mSourceDirectory )) &&
               ( drive.IsReady )) {
                mSourceVolumeName = drive.VolumeLabel;

                OnSourceAvailable();
            }
        }

        private void OnSourceAvailable() {
            mJobSubject.OnNext( new Events.JobTargets( mSourceVolumeName, mSourceDirectory, mTargetDirectory ));
        }

        public void Dispose() {
            StopNotifying();

            mProcessQueueSubscription?.Dispose();
            mProcessQueueSubscription = null;

            mDriveManager?.Dispose();
        }
    }
}