using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using ArchiveLoader.Dto;
using ArchiveLoader.Interfaces;

namespace ArchiveLoader.Models {
    class ProcessManager : IProcessManager {
        private readonly IDriveEjector              mDriveEjector;
        private readonly IDriveManager              mDriveManager;
        private readonly IFileCopier                mFileCopier;
        private readonly IProcessBuilder            mProcessBuilder;
        private readonly IPreferences               mPreferences;
        private readonly IPlatformLog               mLog;
        private DriveInfo                           mSourceDrive;
        private string                              mTargetDirectory;
        private string                              mDriveNotificationKey;
        private CancellationTokenSource             mFileCopyCancellation;
        private readonly List<ProcessItem>          mProcessList;

        private readonly Subject<ProcessItemEvent>  mSubject;
        public  IObservable<ProcessItemEvent>       OnProcessingItemChanged => mSubject;

        public ProcessManager( IProcessBuilder processBuilder, IFileCopier fileCopier, IDriveManager driveManager, IDriveEjector driveEjector, IPreferences preferences, IPlatformLog log ) {
            mProcessBuilder = processBuilder;
            mDriveManager = driveManager;
            mDriveEjector = driveEjector;
            mFileCopier = fileCopier;
            mPreferences = preferences;
            mLog = log;

            mProcessList = new List<ProcessItem>();
            mSubject = new Subject<ProcessItemEvent>();
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
            else {
                if (status.Success) {
                    var item = new ProcessItem( status.FileName );

                    mProcessBuilder.BuildProcessList( item );
                    mProcessList.Add( item );

                    mSubject.OnNext( new ProcessItemEvent( item, EventReason.Add ));

                    PumpProcessHandling();
                }
            }
        }

        private ProcessHandler FindRunnableItem() {
            var retValue = default( ProcessHandler );

            foreach( var item in mProcessList ) {
                retValue = item.FindRunnableProcess();

                if( retValue != null ) {
                    break;
                }
            }

            return retValue;
        }

        private void PumpProcessHandling() {

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
