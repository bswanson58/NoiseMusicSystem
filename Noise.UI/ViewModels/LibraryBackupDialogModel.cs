using System;
using System.ComponentModel;
using System.Globalization;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
    class LibraryBackupDialogModel : DialogModelBase, IDataErrorInfo {
        private readonly ILibraryConfiguration  mLibraryConfiguration;
        private readonly ILibraryBackupManager  mLibraryBackup;
        private readonly IPreferences           mPreferences;
        private LibraryConfiguration            mCurrentLibrary;

        public  ObservableCollectionEx<LibraryConfiguration>   Libraries { get; }

        public LibraryBackupDialogModel( ILibraryBackupManager libraryBackup, ILibraryConfiguration configuration, IPreferences preferences ) {
            mLibraryBackup = libraryBackup;
            mLibraryConfiguration = configuration;
            mPreferences = preferences;

            Libraries = new ObservableCollectionEx<LibraryConfiguration>();
        }

        public void Initialize() {
            Libraries.AddRange( mLibraryConfiguration.Libraries );
            CurrentLibrary = mLibraryConfiguration.Current;

            var corePreferences = mPreferences.Load<NoiseCorePreferences>();

            EnforceBackupCopyLimit = corePreferences.EnforceBackupCopyLimit;
            BackupCopyLimit = corePreferences.MaximumBackupCopies.ToString();

            ProgressAmount = 0;
            ProgressStatus = String.Empty;
            BackupActive = false;
            BackupSucceeded = false;
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

        public bool BackupSucceeded {
            get => Get( () => BackupSucceeded );
            set => Set( () => BackupSucceeded, value );
        }

        public bool EnforceBackupCopyLimit {
            get => Get( () => EnforceBackupCopyLimit );
            set => Set( () => EnforceBackupCopyLimit, value );
        }

        public string BackupCopyLimit {
            get => Get( () => BackupCopyLimit );
            set => Set( () => BackupCopyLimit, value );
        }

        private void OnLibrarySelected() {
            if( CurrentLibrary != null ) {
                var preferences = mPreferences.Load<NoiseCorePreferences>();

                CurrentBackupPressurePercentage = Math.Min( 1.0, (double)CurrentLibrary.BackupPressure / preferences.MaximumBackupPressure );
                CurrentBackupPressure = CurrentLibrary.BackupPressure;
            }
        }

        public async void Execute_BackupLibrary() {
            BackupActive = true;
            BackupSucceeded = await mLibraryBackup.BackupLibrary( OnBackupProgress );

            BackupActive = false;
            OnLibrarySelected();
        }

        private void OnBackupProgress( LibrarianProgressReport progress ) {
            ProgressAmount = progress.Progress;
            ProgressStatus = $"{progress.CurrentItem} - {progress.CurrentPhase}";
        }

        public bool CanExecute_BackupLibrary() {
            return mLibraryConfiguration.Current != null;
        }

        public void SaveOptions() {
            var corePreferences = mPreferences.Load<NoiseCorePreferences>();

            corePreferences.EnforceBackupCopyLimit = EnforceBackupCopyLimit;
            if( Int16.TryParse( BackupCopyLimit, NumberStyles.Integer, CultureInfo.CurrentUICulture, out short max )) {
                corePreferences.MaximumBackupCopies = max;
            }

            mPreferences.Save( corePreferences );
        }

        public string this[string columnName] {
            get {
                switch( columnName ) {
                    case nameof( BackupCopyLimit ):
                        if(!Int16.TryParse( BackupCopyLimit, NumberStyles.Integer, CultureInfo.CurrentUICulture, out _ )) {
                            return "Input must be an integer value.";
                        }
                        break;
                }

                return String.Empty;
            }
        }

        public string Error => String.Empty;
    }
}
