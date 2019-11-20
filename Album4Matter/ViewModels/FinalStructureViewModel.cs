using Album4Matter.Dto;
using Album4Matter.Interfaces;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Album4Matter.ViewModels {
    class FinalStructureViewModel : AutomaticCommandBase, IFinalStructureViewModel {
        private readonly IPreferences           mPreferences;
        private readonly IPlatformDialogService mDialogService;
        private string                          mTargetDirectory;

        public FinalStructureViewModel( IPlatformDialogService dialogService, IPreferences preferences ) {
            mPreferences = preferences;
            mDialogService = dialogService;

            var appPreferences = mPreferences.Load<Album4MatterPreferences>();

            TargetDirectory = appPreferences.TargetDirectory;
        }

        public void SetTargetLayout( TargetAlbumLayout layout ) {
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
