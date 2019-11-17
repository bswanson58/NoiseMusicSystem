using ReusableBits.Mvvm.ViewModelSupport;

namespace ArchiveLoader.Dto {
    public class DisplayedStatusItem : PropertyChangeBase {
        public  string          Name { get; }
        public  ProcessState    Status { get; private set; }
        public  bool            IsLastItem { get; private set; }

        public bool             IsRunning => Status == ProcessState.Running;
        public bool             IsPending => Status == ProcessState.Pending;
        public bool             HasCompleted => Status == ProcessState.Completed;
        public bool             IsAborted => Status == ProcessState.Aborted;
        public bool             HasError => Status == ProcessState.Error;

        public DisplayedStatusItem( string name ) {
            Name = name;

            Status = ProcessState.Pending;
            IsLastItem = false;
        }

        public DisplayedStatusItem( string name, ProcessState status ) :
            this( name ) {
            Status = status;
        }

        public DisplayedStatusItem(string name, ProcessState status, bool isLastItem ) :
            this( name, status ) {
            IsLastItem = isLastItem;
        }

        public void SetLastItem( bool isLastItem ) {
            IsLastItem = isLastItem;

            RaisePropertyChanged( () => IsLastItem );
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
