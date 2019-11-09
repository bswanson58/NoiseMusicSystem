﻿using System;
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
        private string                              mDriveNotificationKey;
        private IDisposable                         mProcessQueueSubscription;

        public IObservable<Events.JobTargets>       OnJobReady => mJobSubject;

        public ProcessReadyNotifier( IDriveManager driveManager, IDriveEjector driveEjector, IPreferences preferences, IPlatformLog log ) {
            mDriveManager = driveManager;
            mDriveEjector = driveEjector;
            mPreferences = preferences;
            mLog = log;

            mJobSubject = new Subject<Events.JobTargets>();
        }

        public async void StartProcessing() {
            var preferences = mPreferences.Load<ArchiveLoaderPreferences>();

            mTargetDirectory = preferences.TargetDirectory;

            if((!String.IsNullOrWhiteSpace(preferences.SourceDirectory )) &&
               ( Directory.Exists(preferences.SourceDirectory ))) {
                mSourceDirectory = preferences.SourceDirectory;

                OnSourceAvailable();
            }
            else {
                var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.Name.Equals( preferences.SourceDrive ));

                if( (drive != null ) &&
                   (!String.IsNullOrWhiteSpace( mTargetDirectory )) &&
                   ( Directory.Exists(mTargetDirectory ))) {
                    mSourceDirectory = drive.Name;

                    await mDriveEjector.OpenDrive( drive.Name[0]);

                    mDriveNotificationKey = mDriveManager.AddDriveNotification( mSourceDirectory, OnDriveNotification );
                }
                else {
                    mLog.LogMessage( "ProcessManager: Cannot start processing." );
                }
            }
        }

        private void OnDriveNotification( DriveInfo drive ) {
            if(( drive.Name.Equals( mSourceDirectory )) &&
               ( drive.IsReady )) {
                OnSourceAvailable();
            }
        }

        private void OnSourceAvailable() {
            mJobSubject.OnNext( new Events.JobTargets( mSourceDirectory, mTargetDirectory ));
        }

        public void Dispose() {
            if(!String.IsNullOrWhiteSpace( mDriveNotificationKey )) {
                mDriveManager?.RemoveDriveNotification( mDriveNotificationKey );

                mDriveNotificationKey = String.Empty;
            }

            mProcessQueueSubscription?.Dispose();
            mProcessQueueSubscription = null;

            mDriveManager?.Dispose();
        }
    }
}