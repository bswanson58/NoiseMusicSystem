using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArchiveLoader.Dto;
using ArchiveLoader.Interfaces;
using ArchiveLoader.Views;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;
using ReusableBits.Ui.Platform;

namespace ArchiveLoader.ViewModels {
    class ConfigurationViewModel : PropertyChangeBase, IDisposable {
        private readonly IDriveManager          mDriveManager;
        private readonly IDriveEjector          mDriveEjector;
        private readonly IPreferences           mPreferences;
        private readonly IPlatformDialogService mPlatformDialogService;
        private readonly IDialogService         mDialogService;
        private readonly IPlatformLog           mLog;
        private string                          mTargetDirectory;
        private string                          mSelectedDrive;

        public  IEnumerable<string>             DriveList => mDriveManager.AvailableDrives;

        public  DelegateCommand                 BrowseTargetDirectory { get; }
        public  DelegateCommand                 OpenTargetDirectory { get; }
        public  DelegateCommand                 EditFileHandlers { get; }
        public  DelegateCommand                 EditPreferences { get; }
        public  DelegateCommand                 OpenDrive { get; }
        public  DelegateCommand                 CloseDrive { get; }

        public ConfigurationViewModel( IDriveManager driveManager, IDriveEjector driveEjector, IPreferences preferences, 
                                       IPlatformDialogService platformDialogService, IPlatformLog log, IDialogService dialogService ) {
            mDriveManager = driveManager;
            mDriveEjector = driveEjector;
            mPreferences = preferences;
            mPlatformDialogService = platformDialogService;
            mDialogService = dialogService;

            mLog = log;

            BrowseTargetDirectory = new DelegateCommand( OnBrowseTargetDirectory );
            OpenTargetDirectory = new DelegateCommand( OnOpenTargetDirectory, CanOpenTargetDirectory );
            EditFileHandlers = new DelegateCommand( OnEditFileHandlers );
            EditPreferences = new DelegateCommand( OnEditPreferences );
            OpenDrive = new DelegateCommand( OnOpenDrive );
            CloseDrive = new DelegateCommand( OnCloseDrive );

            var loaderPreferences = mPreferences.Load<ArchiveLoaderPreferences>();

            mTargetDirectory = loaderPreferences.TargetDirectory;
            mSelectedDrive = DriveList.FirstOrDefault( drive => drive.Equals( loaderPreferences.SourceDrive ));
        }

        public string SelectedDrive {
            get => mSelectedDrive;
            set {
                mSelectedDrive = value;

                var loaderPreferences = mPreferences.Load<ArchiveLoaderPreferences>();

                loaderPreferences.SourceDrive = mSelectedDrive;
                mPreferences.Save( loaderPreferences );
            }
        }
        public string TargetDirectory {
            get => mTargetDirectory;
            set {
                mTargetDirectory = value;

                var loaderPreferences = mPreferences.Load<ArchiveLoaderPreferences>();

                loaderPreferences.TargetDirectory = mTargetDirectory;
                mPreferences.Save( loaderPreferences );

                RaisePropertyChanged( () => TargetDirectory );
                OpenTargetDirectory.RaiseCanExecuteChanged();
            }
        }

        private void OnBrowseTargetDirectory() {
            var path = TargetDirectory;

            if( mPlatformDialogService.SelectFolderDialog( "Select Target Directory", ref path ).GetValueOrDefault( false )) {
                TargetDirectory = path;
            }
        }

        private void OnOpenTargetDirectory() {
            if( Directory.Exists( TargetDirectory )) {
                try {
                    System.Diagnostics.Process.Start( TargetDirectory );
                }
                catch (Exception ex) {
                    mLog.LogException("OnLaunchRequest:Target Directory", ex);
                }
            }
        }

        private bool CanOpenTargetDirectory() {
            return !String.IsNullOrWhiteSpace( TargetDirectory ) && Directory.Exists( TargetDirectory );
        }

        private void OnEditFileHandlers() {
            mDialogService.ShowDialog( typeof( FileHandlerDialogView ).Name, new DialogParameters(), OnFileHandlerEditCompleted );
        }

        private void OnFileHandlerEditCompleted( IDialogResult result ) {}

        private void OnEditPreferences() {
            mDialogService.ShowDialog( typeof( PreferencesDialogView ).Name, new DialogParameters(), OnPreferencesEditCompleted );
        }

        private void OnPreferencesEditCompleted( IDialogResult result ) {}

        private void OnOpenDrive() {
            if(!String.IsNullOrWhiteSpace( SelectedDrive )) {
                mDriveEjector.OpenDrive( SelectedDrive );
            }
        }

        private void OnCloseDrive() {
            if (!String.IsNullOrWhiteSpace( SelectedDrive )) {
                mDriveEjector.CloseDrive( SelectedDrive );
            }
        }

        public void Dispose() {
            mDriveManager?.Dispose();
        }
    }
}
