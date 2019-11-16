using System;
using ArchiveLoader.Interfaces;
using ReusableBits.Mvvm.ViewModelSupport;

namespace ArchiveLoader.ViewModels {
    class ProcessControlViewModel : AutomaticCommandBase, IDisposable {
        private readonly IProcessManager    mProcessManager;
        private IDisposable                 mProcessingStateSubscription;
        private ProcessingState             mProcessingState;

        public ProcessControlViewModel( IProcessManager processManager ) {
            mProcessManager = processManager;

            mProcessingState = ProcessingState.Stopped;

            mProcessingStateSubscription = mProcessManager.OnProcessingStateChanged.Subscribe( OnProcessingStateChanged );
        }

        private void OnProcessingStateChanged( ProcessingState state ) {
            mProcessingState = state;

            RaiseCanExecuteChangedEvent( "CanExecute_StartProcessing" );
            RaiseCanExecuteChangedEvent( "CanExecute_StopProcessing" );
        }

        public void Execute_StartProcessing() {
            mProcessManager.StartProcessing();
        }

        public bool CanExecute_StartProcessing() {
            return mProcessingState != ProcessingState.Running;
        }

        public void Execute_StopProcessing() {
            mProcessManager.StopProcessing();
        }

        public bool CanExecute_StopProcessing() {
            return mProcessingState == ProcessingState.Running;
        }

        public void Dispose() {
            mProcessingStateSubscription?.Dispose();
            mProcessingStateSubscription = null;
        }
    }
}
