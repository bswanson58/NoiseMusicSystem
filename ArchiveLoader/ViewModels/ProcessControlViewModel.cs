using System;
using ArchiveLoader.Interfaces;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;

namespace ArchiveLoader.ViewModels {
    class ProcessControlViewModel : PropertyChangeBase, IDisposable {
        private readonly IProcessManager    mProcessManager;
        private IDisposable                 mProcessingStateSubscription;
        private ProcessingState             mProcessingState;

        public  DelegateCommand             StartProcessing { get; }
        public  DelegateCommand             StopProcessing { get; }

        public ProcessControlViewModel( IProcessManager processManager ) {
            mProcessManager = processManager;

            StartProcessing = new DelegateCommand( OnStartProcessing, CanStartProcessing );
            StopProcessing = new DelegateCommand( OnStopProcessing, CanStopProcessing );

            mProcessingState = ProcessingState.Stopped;

            mProcessingStateSubscription = mProcessManager.OnProcessingStateChanged.Subscribe( OnProcessingStateChanged );
        }

        private void OnProcessingStateChanged( ProcessingState state ) {
            mProcessingState = state;

            StartProcessing.RaiseCanExecuteChanged();
            StopProcessing.RaiseCanExecuteChanged();
        }

        private void OnStartProcessing() {
            mProcessManager.StartProcessing();
        }

        private bool CanStartProcessing() {
            return mProcessingState != ProcessingState.Running;
        }

        private void OnStopProcessing() {
            mProcessManager.StopProcessing();
        }

        private bool CanStopProcessing() {
            return mProcessingState == ProcessingState.Running;
        }

        public void Dispose() {
            mProcessingStateSubscription?.Dispose();
            mProcessingStateSubscription = null;
        }
    }
}
