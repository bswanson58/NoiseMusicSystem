using System;
using ArchiveLoader.Dto;
using ArchiveLoader.Interfaces;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;
using ReusableBits.Ui.Platform;

namespace ArchiveLoader.ViewModels {
    class PreferencesDialogModel : PropertyChangeBase, IDialogAware {
        private readonly IPreferences           mPreferences;
        private readonly IPlatformDialogService mPlatformDialogService;

        public  string                          CatalogDirectory { get; set; }
        public  string                          ReportDirectory { get; set; }

        public  string                          Title { get; }
        public  event Action<IDialogResult>     RequestClose;

        public  DelegateCommand                 Ok { get; }
        public  DelegateCommand                 Cancel { get; }
        public  DelegateCommand                 BrowseCatalogFolder { get; }
        public  DelegateCommand                 BrowseReportFolder { get; }

        public PreferencesDialogModel( IPlatformDialogService platformDialogService, IPreferences preferences ) {
            mPlatformDialogService = platformDialogService;
            mPreferences = preferences;

            BrowseCatalogFolder = new DelegateCommand( OnBrowseCatalogFolder );
            BrowseReportFolder = new DelegateCommand( OnBrowseReportFolder );
            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );

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

        private void OnBrowseCatalogFolder() {
            var path = CatalogDirectory;

            if( mPlatformDialogService.SelectFolderDialog( "Select Catalog Folder", ref path ).GetValueOrDefault( false )) {
                CatalogDirectory = path;

                RaisePropertyChanged( () => CatalogDirectory );
            }
        }

        private void OnBrowseReportFolder() {
            var path = ReportDirectory;

            if( mPlatformDialogService.SelectFolderDialog( "Select Report Folder", ref path ).GetValueOrDefault( false )) {
                ReportDirectory = path;

                RaisePropertyChanged( () => ReportDirectory );
            }
        }

        private void OnOk() {
            SavePreferences();

            RequestClose?.Invoke( new DialogResult( ButtonResult.OK ));
        }

        private void SavePreferences() {
            var preferences = mPreferences.Load<ArchiveLoaderPreferences>();

            preferences.CatalogDirectory = CatalogDirectory;
            preferences.ReportDirectory = ReportDirectory;

            mPreferences.Save( preferences );
        }

        private void OnCancel() {
            RequestClose?.Invoke( new DialogResult( ButtonResult.Cancel ));
        }
    }
}
