using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using Album4Matter.Dto;
using Album4Matter.Interfaces;
using Caliburn.Micro;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Album4Matter.ViewModels {
    class StructureWorkshopViewModel : AutomaticCommandBase {
        private readonly IPlatformDialogService     mDialogService;
        private readonly IPreferences               mPreferences;
        private string                              mSourceDirectory;
        private BindableCollection<SourceItem>      mSourceList;

        public  ICollectionView                     SourceList { get; }

        public  string                              ArtistName { get; set; }
        public  string                              AlbumName { get; set; }
        public  string                              PublishDate { get; set; }

        public StructureWorkshopViewModel( IPlatformDialogService dialogService, IPreferences preferences ) {
            mDialogService = dialogService;
            mPreferences = preferences;

            mSourceList = new BindableCollection<SourceItem>();
            SourceList = CollectionViewSource.GetDefaultView( mSourceList );

            var appPreferences = mPreferences.Load<Album4MatterPreferences>();

            mSourceDirectory = appPreferences.SourceDirectory;
            CollectRootFolder( mSourceDirectory );
        }

        public void Execute_BrowseSourceFolder() {
            var directory = mSourceDirectory;

            if( mDialogService.SelectFolderDialog( "Select Source Directory", ref directory ) == true ) {
                CollectRootFolder( directory );
            }
        }

        public void Execute_RefreshSourceFolder() {
            CollectRootFolder( mSourceDirectory );
        }

        private void CollectRootFolder( string rootPath ) {
            if( Directory.Exists( rootPath )) {
                var rootFolder = new SourceFolder( rootPath );

                CollectFolder( rootFolder );

                mSourceList.Clear();
                mSourceList.AddRange( rootFolder.Children );
            }
        }

        private void CollectFolder( SourceFolder rootFolder ) {
            foreach( var directory in Directory.GetDirectories( rootFolder.FileName )) {
                var folder = new SourceFolder( directory, rootFolder.Key );

                rootFolder.Children.Add( folder );
                CollectFolder( folder );
            }

            foreach( var file in Directory.EnumerateFiles( rootFolder.FileName )) {
                rootFolder.Children.Add( new SourceFile( file, rootFolder.Key ));
            }
        }
    }
}
