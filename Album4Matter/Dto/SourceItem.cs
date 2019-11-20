using System.IO;
using Album4Matter.Models;
using Caliburn.Micro;

namespace Album4Matter.Dto {
    public class SourceItem {
        public  int     Key {  get; }
        public  int     ParentKey {  get; }
        public  string  Name { get; }
        public  string  FileName { get; }

        protected SourceItem( string name, string filename, int key, int parentKey ) {
            Name = name;
            FileName = filename;
            Key = key;
            ParentKey = parentKey;
        }
    }

    public class SourceFile : SourceItem {
        public SourceFile( string fileName ) :
            base( Path.GetFileName( fileName ), fileName, KeyMaker.Master.MakeKey(), KeyMaker.RootKey ) { }

        public SourceFile( string fileName, int parentKey ) :
            base( Path.GetFileName( fileName ), fileName, KeyMaker.Master.MakeKey(), parentKey ) { }
    }

    public class SourceFolder : SourceItem {
        public  BindableCollection<SourceItem>  Children { get; }

        public SourceFolder( string fileName ) :
            base( Path.GetFileName( fileName ), fileName, KeyMaker.Master.MakeKey(), KeyMaker.RootKey ) {
            Children = new BindableCollection<SourceItem>();
        }

        public SourceFolder( string fileName, int parentKey ) :
            base( Path.GetFileName( fileName ), fileName, KeyMaker.Master.MakeKey(), parentKey ) {
            Children = new BindableCollection<SourceItem>();
        }
    }
}
