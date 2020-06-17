using System;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;

namespace ArchiveLoader.Dto {
    class DisplayedProcessItem : PropertyChangeBase {
        private readonly Action<DisplayedProcessItem> mContinueOnError;
        private readonly Action<DisplayedProcessItem> mOpenAction;
        private readonly Action<DisplayedProcessItem> mAbortAction;

        private string          mCurrentHandler;
        private ProcessState    mCurrentState;

        public  string          Key {  get; }
        public  string          Name { get; }
        public  string          FileName { get; }
        public  string          ProcessOutput { get; private set; }
        public  bool            OutputFilePresent { get; private set; }

        public  bool            IsRunning => CurrentState == ProcessState.Running;
        public  bool            IsPending => CurrentState == ProcessState.Pending;
        public  bool            HasCompleted => CurrentState == ProcessState.Completed;
        public  bool            IsAborted => CurrentState == ProcessState.Aborted;
        public  bool            HasError => CurrentState == ProcessState.Error;

        public  DelegateCommand Continue { get; }
        public  DelegateCommand Abort { get; }
        public  DelegateCommand OpenFolder { get; }

        public DisplayedProcessItem( ProcessItem item, Action<DisplayedProcessItem> onContinuation, Action<DisplayedProcessItem> abortAction, Action<DisplayedProcessItem> openAction ) {
            Key = item.Key;
            Name = item.Name;
            FileName = item.FileName;
            mContinueOnError = onContinuation;
            mAbortAction = abortAction;
            mOpenAction = openAction;

            Continue = new DelegateCommand( OnContinue );
            Abort = new DelegateCommand( OnAbort );
            OpenFolder = new DelegateCommand( OnOpenFolder );

            CurrentHandler = "File Copied";
            CurrentState = ProcessState.Pending;
        }

        public string CurrentHandler {
            get => mCurrentHandler;
            set {
                mCurrentHandler = value;

                RaisePropertyChanged( () => CurrentHandler );
            }
        }

        public ProcessState CurrentState {
            get => mCurrentState;
            set {
                mCurrentState = value;

                RaisePropertyChanged( () => CurrentState );
                RaisePropertyChanged( () => IsPending );
                RaisePropertyChanged( () => IsRunning );
                RaisePropertyChanged( () => IsAborted );
                RaisePropertyChanged( () => HasCompleted );
                RaisePropertyChanged( () => HasError );
                RaisePropertyChanged( () => OutputFilePresent );
            }
        }

        public void SetProcessOutput( string output, bool outputPresent ) {
            ProcessOutput = output;
            OutputFilePresent = outputPresent;

            RaisePropertyChanged( () => ProcessOutput );
            RaisePropertyChanged( () => OutputFilePresent );
        }

        private void OnContinue() {
            mContinueOnError?.Invoke( this );
        }

        private void OnAbort() {
            mAbortAction?.Invoke( this );
        }

        private void OnOpenFolder() {
            mOpenAction?.Invoke( this );
        }
    }
}
