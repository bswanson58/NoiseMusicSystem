using System;
using System.IO;
using ArchiveLoader.Dto;
using ArchiveLoader.Interfaces;
using ArchiveLoader.Platform;
using ReusableBits.Mvvm.ViewModelSupport;

namespace ArchiveLoader.ViewModels {
    class ConfigurationViewModel : AutomaticCommandBase {
        private readonly IPreferences           mPreferences;
        private readonly IPlatformDialogService mDialogService;
        private readonly IPlatformLog           mLog;
        private string                          mTargetDirectory;

        public ConfigurationViewModel( IPreferences preferences, IPlatformDialogService dialogService, IPlatformLog log ) {
            mPreferences = preferences;
            mDialogService = dialogService;
            mLog = log;

            var loaderPreferences = mPreferences.Load<ArchiveLoaderPreferences>();

            mTargetDirectory = loaderPreferences.TargetDirectory;
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
    }
}
