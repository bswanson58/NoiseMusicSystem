using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Logging;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
    class LibraryBackupDialogModel : DialogModelBase {
        private readonly ILibraryConfiguration  mLibraryConfiguration;
        private readonly ILibraryBackupManager  mLibraryBackup;
        private readonly IUiLog                 mLog;
        private LibraryConfiguration            mCurrentLibrary;

        public  ObservableCollectionEx<LibraryConfiguration>   Libraries { get; }

        public LibraryBackupDialogModel( ILibraryBackupManager libraryBackup, ILibraryConfiguration configuration, IUiLog log ) {
            mLibraryBackup = libraryBackup;
            mLibraryConfiguration = configuration;
            mLog = log;

            Libraries = new ObservableCollectionEx<LibraryConfiguration>();
            Libraries.AddRange( mLibraryConfiguration.Libraries );
            CurrentLibrary = mLibraryConfiguration.Current;
        }

        public LibraryConfiguration CurrentLibrary {
            get => mCurrentLibrary;
            set {
                mCurrentLibrary = value;

                OnLibrarySelected();
                RaisePropertyChanged( () => CurrentLibrary );
            }
        }

        public string BackupProgress {
            get => Get( () => BackupProgress );
            set => Set( () => BackupProgress, value );
        }

        private void OnLibrarySelected() { }

        public void Execute_BackupLibrary() {
            mLibraryBackup.BackupLibrary( OnBackupProgress );
        }

        private void OnBackupProgress( LibrarianProgressReport progress ) {
            BackupProgress = progress.CurrentPhase;
        }

        public bool CanExecute_BackupLibrary() {
            return mLibraryConfiguration.Current != null;
        }
    }
}
