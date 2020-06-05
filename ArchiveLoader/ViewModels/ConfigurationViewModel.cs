using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArchiveLoader.Behaviours;
using ArchiveLoader.Dto;
using ArchiveLoader.Interfaces;
using ArchiveLoader.Views;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;
using ReusableBits.Ui.Platform;

namespace ArchiveLoader.ViewModels {
    internal class FileHandlerEditRequest : InteractionRequestData<FileHandlerDialogModel> {
        public FileHandlerEditRequest(FileHandlerDialogModel viewModel) : base(viewModel) { }
    }

    class ConfigurationViewModel : AutomaticCommandBase, IDisposable {
        private readonly IDriveManager          mDriveManager;
        private readonly IDriveEjector          mDriveEjector;
        private readonly IPreferences           mPreferences;
        private readonly IPlatformDialogService mPlatformDialogService;
        private readonly IDialogService         mDialogService;
        private readonly IPlatformLog           mLog;
        private string                          mTargetDirectory;
        private string                          mSelectedDrive;

        public  IEnumerable<string>             DriveList => mDriveManager.AvailableDrives;

        public ConfigurationViewModel( IDriveManager driveManager, IDriveEjector driveEjector, IPreferences preferences, 
                                       IPlatformDialogService platformDialogService, IPlatformLog log, IDialogService dialogService ) {
            mDriveManager = driveManager;
            mDriveEjector = driveEjector;
            mPreferences = preferences;
            mPlatformDialogService = platformDialogService;
            mDialogService = dialogService;

            mLog = log;

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
            }
        }

        public void Execute_BrowseTargetDirectory() {
            var path = TargetDirectory;

            if( mPlatformDialogService.SelectFolderDialog( "Select Target Directory", ref path ).GetValueOrDefault( false )) {
                TargetDirectory = path;
            }
        }

        public void Execute_OpenTargetDirectory() {
            if( Directory.Exists( TargetDirectory )) {
                try {
                    System.Diagnostics.Process.Start( TargetDirectory );
                }
                catch (Exception ex) {
                    mLog.LogException("OnLaunchRequest:Target Directory", ex);
                }
            }
        }

        [DependsUpon("TargetDirectory")]
        public bool CanExecute_OpenStagingFolder() {
            return !String.IsNullOrWhiteSpace( TargetDirectory ) && Directory.Exists( TargetDirectory );
        }

        public void Execute_EditFileHandlers() {
            mDialogService.ShowDialog( typeof( FileHandlerDialogView ).Name, new DialogParameters(), OnFileHandlerEditCompleted );
        }

        private void OnFileHandlerEditCompleted( IDialogResult result ) {}

        public void Execute_EditPreferences() {
            mDialogService.ShowDialog( typeof( PreferencesDialogView ).Name, new DialogParameters(), OnPreferencesEditCompleted );
        }

        private void OnPreferencesEditCompleted( IDialogResult result ) {}

        public void Execute_OpenDrive() {
            if(!String.IsNullOrWhiteSpace( SelectedDrive )) {
                mDriveEjector.OpenDrive( SelectedDrive );
            }
        }

        public void Execute_CloseDrive() {
            if (!String.IsNullOrWhiteSpace( SelectedDrive )) {
                mDriveEjector.CloseDrive( SelectedDrive );
            }
        }

        public void Dispose() {
            mDriveManager?.Dispose();
        }
    }
}
