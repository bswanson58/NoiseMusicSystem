using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using Album4Matter.Dto;
using Album4Matter.Interfaces;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Album4Matter.ViewModels {
    class StructureWorkshopViewModel : AutomaticCommandBase {
        private readonly IPlatformDialogService     mDialogService;
        private readonly IPreferences               mPreferences;
        private string                              mSourceDirectory;
        private ObservableCollection<SourceItem>    mSourceList;

        public  ICollectionView                     SourceList { get; }

        public StructureWorkshopViewModel( IPlatformDialogService dialogService, IPreferences preferences ) {
            mDialogService = dialogService;
            mPreferences = preferences;

            mSourceList = new ObservableCollection<SourceItem>();
            SourceList = CollectionViewSource.GetDefaultView( mSourceList );

            var appPreferences = mPreferences.Load<Album4MatterPreferences>();

            mSourceDirectory = appPreferences.SourceDirectory;
        }

        public void Execute_BrowseFolders() {
            var directory = mSourceDirectory;

            if( mDialogService.SelectFolderDialog( "Select Source Directory", ref directory ) == true ) {
                CollectFolder( directory );
            }
        }

        private void CollectFolder( string rootPath ) {
            var rootFolder = new SourceFolder( rootPath );

            mSourceList.Add( rootFolder );
            CollectFolder( rootFolder );
        }

        private void CollectFolder( SourceFolder rootFolder ) {
            foreach( var directory in Directory.GetDirectories( rootFolder.FileName )) {
                var folder = new SourceFolder( directory, rootFolder.Key );

                mSourceList.Add( folder );
                CollectFolder( folder );
            }

            foreach( var file in Directory.EnumerateFiles( rootFolder.FileName )) {
                mSourceList.Add( new SourceFile( file, rootFolder.Key ));
            }
        }

        public void Execute_BrowseFiles() {
            if( mDialogService.OpenFileDialog( "Select Source Files", "*.*", "All files (*.*)|*.*", out string[] fileNames, mSourceDirectory ) == true ) {
                mSourceList.AddRange( from f in fileNames select new SourceFile( f ));
            }
        }
    }
}
