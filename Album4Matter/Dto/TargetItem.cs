using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Album4Matter.Dto {
    public class TargetItem : AutomaticCommandBase {
        private readonly Action<TargetItem> mOnRemoveItem;

        public  string  Name { get; private set; }

        protected TargetItem( string name, Action<TargetItem> onRemoveItem ) {
            mOnRemoveItem = onRemoveItem;

            UpdateTarget( name );
        }

        public void UpdateTarget( string name ) {
            Name = name;

            RaisePropertyChanged( () => Name );
        }

        public void Execute_RemoveItem() {
            mOnRemoveItem?.Invoke( this );
        }
    }

    [DebuggerDisplay("TargetFile = {" + nameof( Name ) + "}")]
    public class TargetFile : TargetItem {
        public TargetFile( string name, Action<TargetItem> onRemoveItem ) :
            base( name, onRemoveItem ) { }

        public TargetFile( SourceFile source, Action<TargetItem> onRemoveItem ) :
            base( source.Name, onRemoveItem ) {
        }
    }

    [DebuggerDisplay("TargetFolder = {" + nameof( Name ) + "}")]
    public class TargetFolder : TargetItem {
        public  ObservableCollection<TargetItem>    Children {  get; }

        public TargetFolder( string name ) :
            base( name, null ) {

            Children = new ObservableCollection<TargetItem>();
        }

        public TargetFolder( SourceFolder source, Action<TargetItem> onRemoveItem ) :
            this( source.Name ) {

            PopulateChildren( source.Children, onRemoveItem );
        }

        public TargetFolder( TargetVolume source, Action<TargetItem> onRemoveItem ) :
            this( source.VolumeName ) {

            PopulateChildren( source.VolumeContents, onRemoveItem );
        }

        public void PopulateChildren( IEnumerable<SourceItem> source, Action<TargetItem> onRemoveItem ) {
            Children.Clear();

            foreach( var child in source ) {
                if( child is SourceFile file ) {
                    var fileName = file.Name;

                    if(( file.HasTagName ) &&
                       ( file.UseTagNameAsTarget )) {
                        fileName = file.TagName;
                    }

                    Children.Add( new TargetFile( fileName, onRemoveItem ));
                }
                else if( child is SourceFolder folder ) {
                    Children.Add( new TargetFolder( folder, onRemoveItem ));
                }
            }
        }
    }
}
