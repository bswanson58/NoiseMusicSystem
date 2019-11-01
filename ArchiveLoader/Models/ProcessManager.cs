using System;
using System.IO;
using System.Linq;
using ArchiveLoader.Dto;
using ArchiveLoader.Interfaces;

namespace ArchiveLoader.Models {
    class ProcessManager : IProcessManager {
        private readonly IDriveEjector  mDriveEjector;
        private readonly IDriveManager  mDriveManager;
        private readonly IPreferences   mPreferences;
        private readonly IPlatformLog   mLog;
        private DriveInfo               mSourceDrive;
        private string                  mTargetDirectory;

        public ProcessManager( IDriveManager driveManager, IDriveEjector driveEjector, IPreferences preferences, IPlatformLog log ) {
            mDriveManager = driveManager;
            mDriveEjector = driveEjector;
            mPreferences = preferences;
            mLog = log;
        }

        public async void StartProcessing() {
            var preferences = mPreferences.Load<ArchiveLoaderPreferences>();

            mSourceDrive = mDriveManager.AvailableDrives.FirstOrDefault( drive => drive.Name.Equals( preferences.SourceDrive ));
            mTargetDirectory = preferences.TargetDirectory;

            if(( mSourceDrive != null ) &&
               (!String.IsNullOrWhiteSpace( mTargetDirectory )) &&
               ( Directory.Exists( mTargetDirectory ))) {
                await mDriveEjector.OpenDrive( mSourceDrive.Name[0]);

                InitializeProcessing();
            }
        }

        private void InitializeProcessing() {

        }
    }
}
