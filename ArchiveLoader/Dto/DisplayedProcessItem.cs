using System;
using ReusableBits.Mvvm.ViewModelSupport;

namespace ArchiveLoader.Dto {
    class DisplayedProcessItem : AutomaticCommandBase {
        private readonly Action<DisplayedProcessItem>   mContinueOnError;

        private string          mCurrentHandler;
        private ProcessState    mCurrentState;

        public  string          Key {  get; }
        public  string          Name { get; }
        public  string          FileName { get; }
        public  string          ProcessOutput { get; private set; }

        public  bool            IsRunning => CurrentState == ProcessState.Running;
        public  bool            IsPending => CurrentState == ProcessState.Pending;
        public  bool            HasCompleted => CurrentState == ProcessState.Completed;
        public  bool            HasError => CurrentState == ProcessState.Error;

        public DisplayedProcessItem( ProcessItem item, Action<DisplayedProcessItem> onContinuation ) {
            Key = item.Key;
            Name = item.Name;
            FileName = item.FileName;
            mContinueOnError = onContinuation;

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
                RaisePropertyChanged( () => HasCompleted );
                RaisePropertyChanged( () => HasError );
            }
        }

        public void SetProcessOutput( string output ) {
            ProcessOutput = output;

            RaisePropertyChanged( () => ProcessOutput );
        }

        public void Execute_OnContinue() {
            mContinueOnError?.Invoke( this );
        }
    }
}
