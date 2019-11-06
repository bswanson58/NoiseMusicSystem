using System;
using ReusableBits.Mvvm.ViewModelSupport;

namespace ArchiveLoader.Dto {
    class DisplayedProcessItem : PropertyChangeBase {
        private string          mCurrentHandler;
        private ProcessState    mCurrentState;

        public  string          Key {  get; }
        public  string          Name { get; }
        public  string          FileName { get; }

        public  bool            IsRunning => CurrentState == ProcessState.Running;
        public  bool            IsPending => CurrentState == ProcessState.Pending;
        public  bool            HasCompleted => CurrentState == ProcessState.Completed;

        public DisplayedProcessItem( ProcessItem item ) {
            Key = item.Key;
            Name = item.Name;
            FileName = item.FileName;

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
            }
        }
    }
}
