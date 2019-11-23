using System;
using System.Diagnostics;
using System.IO;
using Album4Matter.Models;
using Caliburn.Micro;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Album4Matter.Dto {
    public class SourceItem : AutomaticCommandBase {
        private readonly Action<SourceItem> mInspectAction;

        public  int     Key {  get; }
        public  int     ParentKey {  get; }
        public  string  Name { get; }
        public  string  FileName { get; }

        protected SourceItem( string name, string filename, int key, int parentKey, Action<SourceItem> inspectAction ) {
            mInspectAction = inspectAction;

            Name = name;
            FileName = filename;
            Key = key;
            ParentKey = parentKey;
        }

        public void Execute_InspectItem() {
            mInspectAction?.Invoke( this );
        }
    }

    [DebuggerDisplay("SourceFile = {" + nameof( Name ) + "}")]
    public class SourceFile : SourceItem {
        public  string      TagArtist { get; private set; }
        public  string      TagAlbum { get; private set; }
        public  int         TagIndex { get; private set; }
        public  string      TagTitle { get; private set; }
        public  string      TagName { get; private set; }
        public  bool        HasTagName => !String.IsNullOrWhiteSpace( TagName );

        public SourceFile( string fileName, Action<SourceItem> inspectAction ) :
            base( Path.GetFileName( fileName ), fileName, KeyMaker.Master.MakeKey(), KeyMaker.RootKey, inspectAction ) { }

        public SourceFile( string fileName, int parentKey, Action<SourceItem> inspectAction ) :
            base( Path.GetFileName( fileName ), fileName, KeyMaker.Master.MakeKey(), parentKey, inspectAction ) { }

        public void SetTags( string artist, string album, int index, string title, string name ) {
            TagArtist = artist;
            TagAlbum = album;
            TagIndex = index;
            TagTitle = title;
            TagName = name;

            RaisePropertyChanged( () => TagIndex );
            RaisePropertyChanged( () => TagTitle );
            RaisePropertyChanged( () => TagName );
            RaisePropertyChanged( () => HasTagName );
        }
    }

    [DebuggerDisplay("SourceFolder = {" + nameof( Name ) + "}")]
    public class SourceFolder : SourceItem {
        public  BindableCollection<SourceItem>  Children { get; }

        public SourceFolder( string fileName, Action<SourceItem> inspectAction ) :
            base( Path.GetFileName( fileName ), fileName, KeyMaker.Master.MakeKey(), KeyMaker.RootKey, inspectAction ) {
            Children = new BindableCollection<SourceItem>();
        }

        public SourceFolder( string fileName, int parentKey, Action<SourceItem> inspectAction ) :
            base( Path.GetFileName( fileName ), fileName, KeyMaker.Master.MakeKey(), parentKey, inspectAction ) {
            Children = new BindableCollection<SourceItem>();
        }
    }
}
