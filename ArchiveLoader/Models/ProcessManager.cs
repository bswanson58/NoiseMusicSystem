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
        private readonly IProcessQueue              mProcessQueue;
        private readonly IPreferences               mPreferences;
        private readonly IPlatformLog               mLog;
        private DriveInfo                           mSourceDrive;
        private string                              mSourceDirectory;
        private bool                                mSourceIsDrive;
        private string                              mTargetDirectory;
        private string                              mDriveNotificationKey;
        private CancellationTokenSource             mFileCopyCancellation;
        private IDisposable                         mProcessQueueSubscription;
        private readonly List<ProcessItem>          mProcessList;

        private readonly Subject<ProcessItemEvent>  mSubject;
        public  IObservable<ProcessItemEvent>       OnProcessingItemChanged => mSubject;

        public ProcessManager( IProcessQueue processQueue, IProcessBuilder processBuilder, IFileCopier fileCopier,
                               IDriveManager driveManager, IDriveEjector driveEjector, IPreferences preferences, IPlatformLog log ) {
            mProcessQueue = processQueue;
            mProcessBuilder = processBuilder;
            mDriveManager = driveManager;
            mDriveEjector = driveEjector;
            mFileCopier = fileCopier;
            mPreferences = preferences;
            mLog = log;

            mProcessList = new List<ProcessItem>();
            mSubject = new Subject<ProcessItemEvent>();

            mProcessQueue.OnProcessCompleted.Subscribe( OnProcessQueueItemCompleted );
        }

        public async void StartProcessing() {
            var preferences = mPreferences.Load<ArchiveLoaderPreferences>();

            mTargetDirectory = preferences.TargetDirectory;

            if((!String.IsNullOrWhiteSpace( preferences.SourceDirectory )) &&
               ( Directory.Exists( preferences.SourceDirectory ))) {
                mSourceDirectory = preferences.SourceDirectory;
                mSourceIsDrive = false;

                OnSourceAvailable();
            }
            else {
                mSourceDrive = mDriveManager.AvailableDrives.FirstOrDefault( drive => drive.Name.Equals( preferences.SourceDrive ));
                mSourceIsDrive = true;

                if(( mSourceDrive != null ) &&
                   (!String.IsNullOrWhiteSpace( mTargetDirectory )) &&
                   ( Directory.Exists( mTargetDirectory ))) {
                    mSourceDirectory = mSourceDrive.Name;

                    await mDriveEjector.OpenDrive( mSourceDrive.Name[0]);

                    InitializeProcessing();
                }
            }
        }

        private void InitializeProcessing() {
            mDriveNotificationKey = mDriveManager.AddDriveNotification( mSourceDrive, OnDriveNotification );
        }

        private void OnDriveNotification( DriveInfo drive ) {
            if(( drive.Name.Equals( mSourceDrive.Name )) &&
               ( drive.IsReady )) {
                OnSourceAvailable();
            }
        }

        private void OnSourceAvailable() {
            var progress = new Progress<FileCopyStatus>();

            progress.ProgressChanged += OnFileCopied;

            mFileCopyCancellation = new CancellationTokenSource();
            mFileCopier.CopyFiles( mSourceDirectory, mTargetDirectory, progress, mFileCopyCancellation );
        }

        private async void OnFileCopied( object sender, FileCopyStatus status ) {
            if( status.CopyCompleted ) {
                if( mSourceIsDrive ) {
                    await mDriveEjector.OpenDrive( mSourceDrive.Name[0]);
                }
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
            if( mProcessQueue.CanAddProcessItem()) {
                var handler = FindRunnableItem();

                if( handler != null ) {
                    mProcessQueue.AddProcessItem( handler );
                }
            }
        }

        private void OnProcessQueueItemCompleted( ProcessHandler handler ) {

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
