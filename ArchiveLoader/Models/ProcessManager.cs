using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
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

        private readonly Subject<ProcessItemEvent>  mProcessingEventSubject;
        public  IObservable<ProcessItemEvent>       OnProcessingItemChanged => mProcessingEventSubject;

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
            mProcessingEventSubject = new Subject<ProcessItemEvent>();

            mProcessQueueSubscription = mProcessQueue.OnProcessCompleted.Subscribe( OnProcessQueueItemCompleted );
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
                var drive = DriveInfo.GetDrives().FirstOrDefault( d => d.Name.Equals( preferences.SourceDrive ));
                mSourceIsDrive = true;

                if(( drive != null ) &&
                   (!String.IsNullOrWhiteSpace( mTargetDirectory )) &&
                   ( Directory.Exists( mTargetDirectory ))) {
                    mSourceDirectory = drive.Name;

                    await mDriveEjector.OpenDrive( drive.Name[0]);

                    InitializeProcessing();
                }
                else {
                    mLog.LogMessage( "ProcessManager: Cannot start processing." );
                }
            }
        }

        private void InitializeProcessing() {
            mDriveNotificationKey = mDriveManager.AddDriveNotification( mSourceDirectory, OnDriveNotification );
        }

        private void OnDriveNotification( DriveInfo drive ) {
            if(( drive.Name.Equals( mSourceDirectory )) &&
               ( drive.IsReady )) {
                mSourceDrive = drive;
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
                if( status.Success ) {
                    switch( status.Status ) {
                        case FileCopyState.Discovered:
                            NewFileDiscovered( status );
                            break;

                        case FileCopyState.Copying:
                            UpdateCopyStatus( status.FileName, status.Status );
                            break;

                        case FileCopyState.Completed:
                            UpdateCopyStatus( status.FileName, status.Status );
                            mLog.LogMessage( $"File Copy Completed: {status.FileName}" );

                            PumpProcessHandling();
                            break;
                    }
                }
                else {
                    mLog.LogMessage( $"File was not copied successfully: '{status.FileName}' - Error: '{status.ErrorMessage}'" );
                }
            }
        }

        private void NewFileDiscovered( FileCopyStatus status ) {
            var item = new ProcessItem( status );

            mProcessingEventSubject.OnNext( new ProcessItemEvent( item, EventReason.Add ));
                    
            mProcessBuilder.BuildProcessList( item );

            mProcessingEventSubject.OnNext( new ProcessItemEvent( item, EventReason.Update ));

            lock( mProcessList ) {
                mProcessList.Add( item );
            }
        }

        private void UpdateCopyStatus( string fileName, FileCopyState state ) {
            ProcessItem processItem;

            lock( mProcessList ) {
                processItem = mProcessList.FirstOrDefault( i => i.FileName.Equals( fileName ));
            }

            var fileCopyHandler = processItem?.ProcessList.FirstOrDefault();

            if( fileCopyHandler != null ) {
                switch( state ) {
                    case FileCopyState.Copying:
                        fileCopyHandler.SetProcessRunning();
                        break;

                    case FileCopyState.Completed:
                        fileCopyHandler.SetProcessOutput( String.Empty, String.Empty, 0 );
                        break;
                }

                mProcessingEventSubject.OnNext( new ProcessItemEvent( processItem, EventReason.Update ));

                if( processItem.HasCompletedProcessing()) {
                    DeleteProcessingItem( processItem );
                }
            }
        }


        private ProcessHandler FindRunnableItem() {
            var retValue = default( ProcessHandler );

            lock( mProcessList ) {
                foreach( var item in mProcessList ) {
                    retValue = item.FindRunnableProcess();

                    if( retValue != null ) {
                        mLog.LogMessage( $"Runnable Item: '{retValue.InputFile}' is {retValue.ProcessState}" );
                        break;
                    }
                }
            }

            return retValue;
        }

        private void PumpProcessHandling() {
            if( mProcessQueue.CanAddProcessItem()) {
                var handler = FindRunnableItem();

                if( handler != null ) {
                    mProcessQueue.AddProcessItem( handler );

                    var item = FindProcessForHandler( handler );

                    if( item != null ) {
                        mProcessingEventSubject.OnNext( new ProcessItemEvent( item, EventReason.Update ));
                    }
                }
            }
        }

        private void OnProcessQueueItemCompleted( ProcessHandler handler ) {
            ProcessItem processItem;
            
            lock( mProcessList ) {
                processItem = mProcessList.FirstOrDefault( i => i.Key.Equals( handler.ParentKey ));
            }

            if( processItem != null ) {
                mProcessingEventSubject.OnNext( new ProcessItemEvent( processItem, EventReason.Update ));

                if( processItem.HasCompletedProcessing()) {
                    DeleteProcessingItem( processItem );
                }
            }

            PumpProcessHandling();
        }

        private void DeleteProcessingItem( ProcessItem item ) {
            Task.Run( async () => {
                await Task.Delay( 750 );

                lock( mProcessList ) {
                    mProcessList.Remove( item );
                }

                mProcessingEventSubject.OnNext( new ProcessItemEvent( item, EventReason.Completed ));
            });
        }

        private ProcessItem FindProcessForHandler( ProcessHandler handler ) {
            ProcessItem retValue;

            lock( mProcessList ) {
                retValue = mProcessList.FirstOrDefault( i => i.Key.Equals( handler.ParentKey ));
            }

            return retValue;
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
