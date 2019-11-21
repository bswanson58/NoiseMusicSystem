using System.Collections.ObjectModel;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Album4Matter.Dto {
    public class TargetItem : PropertyChangeBase {
        public  string  Name { get; private set; }

        protected TargetItem( string name ) {
            UpdateTarget( name );
        }

        public void UpdateTarget( string name ) {
            Name = name;

            RaisePropertyChanged( () => Name );
        }
    }

    public class TargetFile : TargetItem {
        public TargetFile( string name ) :
            base( name ) { }
    }

    public class TargetFolder : TargetItem {
        public  ObservableCollection<TargetItem>    Children {  get; }

        public TargetFolder( string name ) :
            base( name ) {
            Children = new ObservableCollection<TargetItem>();
        }
    }
}
