using System;
using System.IO;
using System.Linq;
using System.Threading;
using ArchiveLoader.Dto;
using ArchiveLoader.Interfaces;

namespace ArchiveLoader.Models {
    class ProcessManager : IProcessManager {
        private readonly IDriveEjector  mDriveEjector;
        private readonly IDriveManager  mDriveManager;
        private readonly IFileCopier    mFileCopier;
        private readonly IPreferences   mPreferences;
        private readonly IPlatformLog   mLog;
        private DriveInfo               mSourceDrive;
        private string                  mTargetDirectory;
        private string                  mDriveNotificationKey;
        private CancellationTokenSource mFileCopyCancellation;

        public ProcessManager( IFileCopier fileCopier, IDriveManager driveManager, IDriveEjector driveEjector, IPreferences preferences, IPlatformLog log ) {
            mDriveManager = driveManager;
            mDriveEjector = driveEjector;
            mFileCopier = fileCopier;
            mPreferences = preferences;
            mLog = log;
        }

        public async void StartProcessing() {
            var preferences = mPreferences.Load<ArchiveLoaderPreferences>();

            mSourceDrive = mDriveManager.AvailableDrives.FirstOrDefault( drive => drive.Name.Equals( preferences.SourceDrive ));
            mTargetDirectory = preferences.TargetDirectory;

            if(( mSourceDrive != null ) &&
               (!String.IsNullOrWhiteSpace( mTargetDirectory )) &&
               ( Directory.Exists( mTargetDirectory ))) {
                await mDriveEjector.OpenDrive( mSourceDrive.Name[0]);

                InitializeProcessing();
            }
        }

        private void InitializeProcessing() {
            mDriveNotificationKey = mDriveManager.AddDriveNotification( mSourceDrive, OnDriveNotification );
        }

        private void OnDriveNotification( DriveInfo drive ) {
            if(( drive.Name.Equals( mSourceDrive.Name )) &&
               ( drive.IsReady )) {
                var progress = new Progress<FileCopyStatus>();

                progress.ProgressChanged += OnFileCopied;

                mFileCopyCancellation = new CancellationTokenSource();
                mFileCopier.CopyFiles( drive.RootDirectory.FullName, mTargetDirectory, progress, mFileCopyCancellation );
            }
        }

        private async void OnFileCopied( object sender, FileCopyStatus status ) {
            if( status.CopyCompleted ) {
                await mDriveEjector.OpenDrive( mSourceDrive.Name[0]);
            }
        }

        public void Dispose() {
            if(!String.IsNullOrWhiteSpace( mDriveNotificationKey )) {
                mDriveManager?.RemoveDriveNotification( mDriveNotificationKey );

                mDriveNotificationKey = String.Empty;
            }

            mDriveManager?.Dispose();
        }
    }
}
