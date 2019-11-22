using System.Collections.Generic;
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

        public TargetFile( SourceFile source ) :
            base( source.Name ) {
        }
    }

    public class TargetFolder : TargetItem {
        public  ObservableCollection<TargetItem>    Children {  get; }

        public TargetFolder( string name ) :
            base( name ) {

            Children = new ObservableCollection<TargetItem>();
        }

        public TargetFolder( SourceFolder source ) :
            this( source.Name ) {

            PopulateChildren( source.Children );
        }

        public void PopulateChildren( IEnumerable<SourceItem> source ) {
            Children.Clear();

            foreach( var child in source ) {
                if( child is SourceFile file ) {
                    Children.Add( new TargetFile( file ));
                }
                else if( child is SourceFolder folder ) {
                    Children.Add( new TargetFolder( folder ));
                }
            }
        }
    }
}
