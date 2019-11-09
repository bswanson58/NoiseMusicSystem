using System;
using System.Reactive.Subjects;
using ArchiveLoader.Dto;
using ArchiveLoader.Interfaces;

namespace ArchiveLoader.Models {
    class ProcessManager : IProcessManager {
        private readonly IProcessReadyNotifier      mReadyNotifier;
        private readonly ICopyProcessor             mCopyProcessor;
        private readonly Subject<Events.ProcessItemEvent>   mCopyEventSubject;
        private IDisposable                         mReadyNotifierSubscription;
        private IDisposable                         mCopyEventSubscription;

        public IObservable<Events.ProcessItemEvent> OnProcessingItemChanged => mCopyEventSubject;

        public ProcessManager( IProcessReadyNotifier readyNotifier, ICopyProcessor copyProcessor ) {
            mReadyNotifier = readyNotifier;
            mCopyProcessor = copyProcessor;

            mCopyEventSubject = new Subject<Events.ProcessItemEvent>();
        }

        public void StartProcessing() {
            mCopyEventSubscription = mCopyProcessor.OnProcessingItemChanged.Subscribe( OnCopyProcessEvent );
            mReadyNotifierSubscription = mReadyNotifier.OnJobReady.Subscribe( OnJobReady );

            mReadyNotifier.StartProcessing();
        }

        private void OnJobReady( Events.JobTargets targets ) {
            mCopyProcessor.StartCopyProcess( targets );
        }

        private void OnCopyProcessEvent( Events.ProcessItemEvent args ) {
            mCopyEventSubject.OnNext( args );
        }

        public void Dispose() {
            mReadyNotifierSubscription?.Dispose();
            mReadyNotifierSubscription = null;

            mCopyEventSubscription?.Dispose();
            mCopyEventSubscription = null;
        }
    }
}
