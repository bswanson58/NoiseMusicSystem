using System;
using Album4Matter.Dto;
using Album4Matter.Interfaces;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Album4Matter.ViewModels {
    class StructureWorkshopViewModel : AutomaticCommandBase {
        private readonly IPlatformDialogService mDialogService;
        private readonly IPreferences           mPreferences;
        private string                          mSourceDirectory;

        public StructureWorkshopViewModel( IPlatformDialogService dialogService, IPreferences preferences ) {
            mDialogService = dialogService;
            mPreferences = preferences;

            var appPreferences = mPreferences.Load<Album4MatterPreferences>();

            mSourceDirectory = appPreferences.SourceDirectory;
        }

        public void Execute_BrowseFolders() {
            var directory = mSourceDirectory;

            if( mDialogService.SelectFolderDialog( "Select Source Directory", ref directory ) == true ) {

            }
        }

        public void Execute_BrowseFiles() {
            var fileNames = String.Empty;

            if( mDialogService.OpenFileDialog( "Select Source Files", "*.*", "All files (*.*)|*.*", out fileNames, mSourceDirectory, true ) == true ) {

            }
        }
    }
}
