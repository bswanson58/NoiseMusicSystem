using ReusableBits.Mvvm.ViewModelSupport;

namespace ArchiveLoader.Dto {
    public class DisplayedStatusItem : PropertyChangeBase {
        public  string          Name { get; }
        public  ProcessState    Status { get; private set; }

        public bool             IsRunning => Status == ProcessState.Running;
        public bool             IsPending => Status == ProcessState.Pending;
        public bool             HasCompleted => Status == ProcessState.Completed;
        public bool             IsAborted => Status == ProcessState.Aborted;
        public bool             HasError => Status == ProcessState.Error;

        public DisplayedStatusItem( string name ) {
            Name = name;

            Status = ProcessState.Pending;
        }

        public void UpdateStatus( ProcessState status ) {
            Status = status;

            RaisePropertyChanged(() => Status );
            RaisePropertyChanged(() => IsPending);
            RaisePropertyChanged(() => IsRunning);
            RaisePropertyChanged(() => IsAborted);
            RaisePropertyChanged(() => HasCompleted);
            RaisePropertyChanged(() => HasError);
        }
    }
}
