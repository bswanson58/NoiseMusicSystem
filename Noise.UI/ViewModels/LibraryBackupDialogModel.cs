using System;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Logging;
using Noise.UI.Support;
using ReactiveUI;

namespace Noise.UI.ViewModels {
    class LibraryBackupDialogModel : DialogModelBase {
        private readonly ILibraryConfiguration  mLibraryConfiguration;
        private readonly ILibraryBackupManager  mLibraryBackup;
        private readonly IPreferences           mPreferences;
        private readonly IUiLog                 mLog;
        private LibraryConfiguration            mCurrentLibrary;

        public  ObservableCollectionEx<LibraryConfiguration>   Libraries { get; }

        public LibraryBackupDialogModel( ILibraryBackupManager libraryBackup, ILibraryConfiguration configuration, IPreferences preferences, IUiLog log ) {
            mLibraryBackup = libraryBackup;
            mLibraryConfiguration = configuration;
            mPreferences = preferences;
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

        public uint CurrentBackupPressure {
            get => Get( () => CurrentBackupPressure );
            set => Set( () => CurrentBackupPressure, value );
        }

        public double CurrentBackupPressurePercentage {
            get => Get( () => CurrentBackupPressurePercentage );
            set => Set( () => CurrentBackupPressurePercentage, value );
        }

        public string ProgressStatus {
            get => Get( () => ProgressStatus );
            set => Set( () => ProgressStatus, value );
        }

        public int ProgressAmount {
            get => Get( () => ProgressAmount );
            set => Set( () => ProgressAmount, value );
        }

        public bool BackupActive {
            get => Get( () => BackupActive );
            set => Set( () => BackupActive, value );
        }

        private void OnLibrarySelected() {
            if( CurrentLibrary != null ) {
                var preferences = mPreferences.Load<NoiseCorePreferences>();

                CurrentBackupPressurePercentage = Math.Min( 1.0, (double)CurrentLibrary.BackupPressure / preferences.MaximumBackupPressure );
                CurrentBackupPressure = CurrentLibrary.BackupPressure;
            }
        }

        public async void Execute_BackupLibrary() {
            await mLibraryBackup.BackupLibrary( OnBackupProgress );

            OnLibrarySelected();
        }

        private void OnBackupProgress( LibrarianProgressReport progress ) {
            ProgressAmount = progress.Progress;
            ProgressStatus = $"{progress.CurrentItem} - {progress.CurrentPhase}";

            BackupActive = !progress.Completed;
        }

        public bool CanExecute_BackupLibrary() {
            return mLibraryConfiguration.Current != null;
        }
    }
}
