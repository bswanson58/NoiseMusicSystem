using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArchiveLoader.Dto;
using ArchiveLoader.Interfaces;
using ArchiveLoader.Platform;
using ReusableBits.Mvvm.ViewModelSupport;

namespace ArchiveLoader.ViewModels {
    class ConfigurationViewModel : AutomaticCommandBase, IDisposable {
        private readonly IDriveManager          mDriveManager;
        private readonly IPreferences           mPreferences;
        private readonly IPlatformDialogService mDialogService;
        private readonly IPlatformLog           mLog;
        private string                          mTargetDirectory;
        private DriveInfo                       mSelectedDrive;

        public  IEnumerable<DriveInfo>          DriveList => mDriveManager.AvailableDrives;

        public ConfigurationViewModel( IDriveManager driveManager, IPreferences preferences, IPlatformDialogService dialogService, IPlatformLog log ) {
            mDriveManager = driveManager;
            mPreferences = preferences;
            mDialogService = dialogService;
            mLog = log;

            var loaderPreferences = mPreferences.Load<ArchiveLoaderPreferences>();

            mTargetDirectory = loaderPreferences.TargetDirectory;
            mSelectedDrive = DriveList.FirstOrDefault( drive => drive.Name.Equals( loaderPreferences.SourceDrive ));
        }

        public DriveInfo SelectedDrive {
            get => mSelectedDrive;
            set {
                mSelectedDrive = value;

                var loaderPreferences = mPreferences.Load<ArchiveLoaderPreferences>();

                loaderPreferences.SourceDrive = mSelectedDrive?.Name;
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

            if( mDialogService.SelectFolderDialog( "Select Target Directory", ref path ).GetValueOrDefault( false )) {
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

        public void Dispose() {
            mDriveManager?.Dispose();
        }
    }
}
