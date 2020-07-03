using System;
using System.Reactive.Subjects;
using ArchiveLoader.Dto;
using ArchiveLoader.Interfaces;

namespace ArchiveLoader.Models {
    class ProcessManager : IProcessManager {
        private readonly IProcessReadyNotifier      mReadyNotifier;
        private readonly ICopyProcessor             mCopyProcessor;
        private readonly IProcessRecorder           mProcessRecorder;
        private readonly Subject<Events.ProcessItemEvent>   mCopyEventSubject;
        private readonly Subject<ProcessingState>           mProcessingStateSubject;
        private IDisposable                         mReadyNotifierSubscription;
        private IDisposable                         mCopyEventSubscription;
        private string                              mCurrentSourceDirectory;

        public IObservable<ProcessingState>         OnProcessingStateChanged => mProcessingStateSubject;
        public IObservable<Events.ProcessItemEvent> OnProcessingItemChanged => mCopyEventSubject;

        public ProcessManager( IProcessReadyNotifier readyNotifier, ICopyProcessor copyProcessor, IProcessRecorder processRecorder ) {
            mReadyNotifier = readyNotifier;
            mCopyProcessor = copyProcessor;
            mProcessRecorder = processRecorder;

            mCopyEventSubject = new Subject<Events.ProcessItemEvent>();
            mProcessingStateSubject = new Subject<ProcessingState>();
        }

        public void StartProcessing() {
            mCopyEventSubscription = mCopyProcessor.OnProcessingItemChanged.Subscribe( OnCopyProcessEvent );
            mReadyNotifierSubscription = mReadyNotifier.OnJobReady.Subscribe( OnJobReady );

            mReadyNotifier.StartNotifying();
            mProcessingStateSubject.OnNext( ProcessingState.Running );
        }

        public void StopProcessing() {
            mReadyNotifier.StopNotifying();

            mProcessingStateSubject.OnNext( ProcessingState.Stopped );
        }

        public void ContinueAllProcesses() {
            mCopyProcessor.ContinueAllProcesses();
        }

        public void ContinueErroredProcess( string processKey, string handlerName ) {
            mCopyProcessor.ContinueErroredProcess( processKey, handlerName );
        }

        public void AbortAllProcesses() {
            mCopyProcessor.AbortAllProcesses();
        }

        public void AbortErroredProcess( string processKey, string handlerName ) {
            mCopyProcessor.AbortErroredProcess( processKey, handlerName );
        }

        private void OnJobReady( Events.JobTargets targets ) {
            mCurrentSourceDirectory = targets.SourceDirectory;

            mProcessRecorder.JobStarted( targets );
            mCopyProcessor.StartCopyProcess( targets );
        }

        private void OnCopyProcessEvent( Events.ProcessItemEvent args ) {
            mCopyEventSubject.OnNext( args );

            if( args.Reason == CopyProcessEventReason.Completed ) {
                mProcessRecorder.ItemCompleted( args.Item );
            }
            else if( args.Reason == CopyProcessEventReason.CopyCompleted ) {
                mReadyNotifier.JobCompleted( mCurrentSourceDirectory );
            }
            else if( args.Reason == CopyProcessEventReason.ProcessingCompleted ) {
                mProcessRecorder.JobCompleted( args.Item.VolumeName );
            }
        }

        public void Dispose() {
            mReadyNotifierSubscription?.Dispose();
            mReadyNotifierSubscription = null;

            mCopyEventSubscription?.Dispose();
            mCopyEventSubscription = null;
        }
    }
}
