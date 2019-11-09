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
        private IDisposable                         mReadyNotifierSubscription;
        private IDisposable                         mCopyEventSubscription;

        public IObservable<Events.ProcessItemEvent> OnProcessingItemChanged => mCopyEventSubject;

        public ProcessManager( IProcessReadyNotifier readyNotifier, ICopyProcessor copyProcessor, IProcessRecorder processRecorder ) {
            mReadyNotifier = readyNotifier;
            mCopyProcessor = copyProcessor;
            mProcessRecorder = processRecorder;

            mCopyEventSubject = new Subject<Events.ProcessItemEvent>();
        }

        public void StartProcessing() {
            mCopyEventSubscription = mCopyProcessor.OnProcessingItemChanged.Subscribe( OnCopyProcessEvent );
            mReadyNotifierSubscription = mReadyNotifier.OnJobReady.Subscribe( OnJobReady );

            mReadyNotifier.StartProcessing();
        }

        private void OnJobReady( Events.JobTargets targets ) {
            mProcessRecorder.JobStarted( targets );
            mCopyProcessor.StartCopyProcess( targets );
        }

        private void OnCopyProcessEvent( Events.ProcessItemEvent args ) {
            mCopyEventSubject.OnNext( args );

            if( args.Reason == CopyProcessEventReason.Completed ) {
                mProcessRecorder.ItemCompleted( args.Item );
            }
            else if( args.Reason == CopyProcessEventReason.CopyCompleted ) {
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
