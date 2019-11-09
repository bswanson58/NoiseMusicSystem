using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using ArchiveLoader.Dto;
using ArchiveLoader.Interfaces;

namespace ArchiveLoader.Models {
    class CopyProcessor : ICopyProcessor {
        private readonly IFileCopier        mFileCopier;
        private readonly IProcessBuilder    mProcessBuilder;
        private readonly IProcessQueue      mProcessQueue;
        private readonly IPlatformLog       mLog;
        private readonly IDisposable        mProcessQueueSubscription;
        private readonly List<ProcessItem>  mProcessList;
        private string                      mSourceDirectory;
        private string                      mTargetDirectory;
        private CancellationTokenSource     mFileCopyCancellation;

        private readonly Subject<Events.ProcessItemEvent>   mProcessingEventSubject;
        public  IObservable<Events.ProcessItemEvent>        OnProcessingItemChanged => mProcessingEventSubject;

        public CopyProcessor( IProcessQueue processQueue, IProcessBuilder processBuilder, IFileCopier fileCopier, IPlatformLog log ) {
            mProcessQueue = processQueue;
            mProcessBuilder = processBuilder;
            mFileCopier = fileCopier;
            mLog = log;

            mProcessList = new List<ProcessItem>();
            mProcessingEventSubject = new Subject<Events.ProcessItemEvent>();

            mProcessQueueSubscription = mProcessQueue.OnProcessCompleted.Subscribe( OnProcessQueueItemCompleted );
        }

        public void StartCopyProcess( Events.JobTargets targets ) {
            mSourceDirectory = targets.SourceDirectory;
            mTargetDirectory = targets.TargetDirectory;
            mFileCopyCancellation = new CancellationTokenSource();

            mFileCopier.CopyFiles( mSourceDirectory, mTargetDirectory, OnFileCopied, mFileCopyCancellation );
        }

        private void OnFileCopied( FileCopyStatus status ) {
            if( status.CopyCompleted ) {
                mProcessingEventSubject.OnNext( new Events.ProcessItemEvent( null, CopyProcessEventReason.CopyCompleted ));
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

            mProcessBuilder.BuildProcessList( item );

            lock( mProcessList ) {
                mProcessList.Add( item );
            }

            mProcessingEventSubject.OnNext( new Events.ProcessItemEvent( item, CopyProcessEventReason.Add ));
        }

        private void UpdateCopyStatus( string fileName, FileCopyState state ) {
            ProcessItem processItem;

            lock( mProcessList ) {
                processItem = mProcessList.FirstOrDefault(i => i.FileName.Equals( fileName ));
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

                mProcessingEventSubject.OnNext( new Events.ProcessItemEvent(processItem, CopyProcessEventReason.Update ));

                if( processItem.HasCompletedProcessing()) {
                    DeleteProcessingItem( processItem );
                }
            }
            else {
                mLog.LogMessage( $"Copy handler could not be found for: '{fileName}'" );
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
                        mProcessingEventSubject.OnNext( new Events.ProcessItemEvent( item, CopyProcessEventReason.Update ));
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
                mProcessingEventSubject.OnNext( new Events.ProcessItemEvent( processItem, CopyProcessEventReason.Update ));

                if( processItem.HasCompletedProcessing()) {
                    DeleteProcessingItem( processItem );
                }
            }

            PumpProcessHandling();
        }

        private void DeleteProcessingItem( ProcessItem item ) {
            Task.Run(async () => {
                await Task.Delay(750);

                lock (mProcessList) {
                    mProcessList.Remove(item);
                }

                mProcessingEventSubject.OnNext( new Events.ProcessItemEvent( item, CopyProcessEventReason.Completed ));
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
            mFileCopyCancellation?.Dispose();
            mProcessQueueSubscription?.Dispose();
            mProcessingEventSubject?.Dispose();
        }
    }
}
