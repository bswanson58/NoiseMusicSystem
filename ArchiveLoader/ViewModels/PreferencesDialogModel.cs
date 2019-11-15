using System;
using ArchiveLoader.Dto;
using ArchiveLoader.Interfaces;
using ArchiveLoader.Platform;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace ArchiveLoader.ViewModels {
    class PreferencesDialogModel : AutomaticCommandBase, IDialogAware {
        private readonly IPreferences           mPreferences;
        private readonly IPlatformDialogService mPlatformDialogService;

        public  string                          CatalogDirectory { get; set; }
        public  string                          ReportDirectory { get; set; }

        public  string                          Title { get; }
        public  event Action<IDialogResult>     RequestClose;

        public PreferencesDialogModel( IPlatformDialogService platformDialogService, IPreferences preferences ) {
            mPlatformDialogService = platformDialogService;
            mPreferences = preferences;

            Title = "Preferences";
        }

        public void OnDialogOpened(IDialogParameters parameters) {
            var preferences = mPreferences.Load<ArchiveLoaderPreferences>();

            CatalogDirectory = preferences.CatalogDirectory;
            ReportDirectory = preferences.ReportDirectory;

            RaisePropertyChanged( () => CatalogDirectory );
            RaisePropertyChanged( () => ReportDirectory );
        }

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogClosed() {}

        public void Execute_BrowseCatalogFolder() {
            var path = CatalogDirectory;

            if( mPlatformDialogService.SelectFolderDialog( "Select Catalog Folder", ref path ).GetValueOrDefault( false )) {
                CatalogDirectory = path;

                RaisePropertyChanged( () => CatalogDirectory );
            }
        }

        public void Execute_BrowseReportFolder() {
            var path = ReportDirectory;

            if( mPlatformDialogService.SelectFolderDialog( "Select Report Folder", ref path ).GetValueOrDefault( false )) {
                ReportDirectory = path;

                RaisePropertyChanged( () => ReportDirectory );
            }
        }

        public void Execute_OnOk() {
            SavePreferences();

            RequestClose?.Invoke( new DialogResult( ButtonResult.OK ));
        }

        private void SavePreferences() {
            var preferences = mPreferences.Load<ArchiveLoaderPreferences>();

            preferences.CatalogDirectory = CatalogDirectory;
            preferences.ReportDirectory = ReportDirectory;

            mPreferences.Save( preferences );
        }

        public void Execute_OnCancel() {
            RequestClose?.Invoke( new DialogResult( ButtonResult.Cancel ));
        }
    }
}
