using System.Collections.ObjectModel;
using Album4Matter.Dto;
using Album4Matter.Interfaces;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Album4Matter.ViewModels {
    class FinalStructureViewModel : AutomaticCommandBase, IFinalStructureViewModel {
        private readonly IPreferences               mPreferences;
        private readonly IPlatformDialogService     mDialogService;
        private readonly TargetFolder               mArtistTarget;
        private readonly TargetFolder               mAlbumTarget;
        private string                              mTargetDirectory;

        public  ObservableCollection<TargetItem>    TargetList { get; }

        public FinalStructureViewModel( IPlatformDialogService dialogService, IPreferences preferences ) {
            mPreferences = preferences;
            mDialogService = dialogService;

            mArtistTarget = new TargetFolder( "Unnamed" );
            mAlbumTarget = new TargetFolder( "Unnamed" );
            TargetList = new ObservableCollection<TargetItem> { mArtistTarget };

            mArtistTarget.Children.Add( mAlbumTarget );

            var appPreferences = mPreferences.Load<Album4MatterPreferences>();

            TargetDirectory = appPreferences.TargetDirectory;
        }

        public void SetTargetLayout( TargetAlbumLayout layout ) {
            mArtistTarget.UpdateTarget( layout.ArtistName );
            mAlbumTarget.UpdateTarget( layout.AlbumName );

            mAlbumTarget.Children.Clear();
            mAlbumTarget.PopulateChildren( layout.AlbumList.VolumeContents );
        }

        public string TargetDirectory {
            get => mTargetDirectory;
            set {
                mTargetDirectory = value;

                RaisePropertyChanged( () => TargetDirectory );
            }
        }

        public void Execute_BrowseTargetFolder() {
            var directory = TargetDirectory;

            if( mDialogService.SelectFolderDialog( "Select Target Directory", ref directory ) == true ) {
                TargetDirectory = directory;

                var appPreferences = mPreferences.Load<Album4MatterPreferences>();

                appPreferences.TargetDirectory = TargetDirectory;
                mPreferences.Save( appPreferences );
            }
        }
    }
}
