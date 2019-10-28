using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using ReusableBits.Mvvm.ViewModelSupport;
using TuneArchiver.Interfaces;
using TuneArchiver.Models;
using TuneArchiver.Platform;

namespace TuneArchiver.ViewModels {
    class ArchiveCreatorViewModel : AutomaticCommandBase {
        private readonly IDirectoryScanner      mDirectoryScanner;
        private readonly ISetCreator            mSetCreator;
        private readonly IArchiveBuilder        mArchiveBuilder;
        private readonly IPreferences           mPreferences;
        private readonly IPlatformDialogService mDialogService;
        private readonly IPlatformLog           mLog;

        public  ObservableCollection<Album>     StagingList { get; }
        private string                          mStagingPath;
        public  int                             StagingCount { get; private set; }
        public  long                            StagingSize { get; private set; }

        public  ObservableCollection<Album>     SelectedList { get; }
        public  int                             SelectedCount { get; private set; }
        public  long                            SelectedSize { get; private set; }

        public  ObservableCollection<string>    ArchiveList { get; }
        private string                          mArchivePath;
        public  string                          ArchiveLabel { get; private set; }
        private string                          mArchiveLabelFormat;
        private string                          mArchiveLabelIdentifier;

        public ArchiveCreatorViewModel( IDirectoryScanner directoryScanner, ISetCreator setCreator, IArchiveBuilder archiveBuilder, IPreferences preferences, 
                                        IPlatformDialogService dialogService, IPlatformLog log ) {
            mDirectoryScanner = directoryScanner;
            mSetCreator = setCreator;
            mArchiveBuilder = archiveBuilder;
            mPreferences = preferences;
            mDialogService = dialogService;
            mLog = log;

            StagingList = new ObservableCollection<Album>();
            SelectedList = new ObservableCollection<Album>();
            ArchiveList = new ObservableCollection<string>();

            var archivePreferences = mPreferences.Load<ArchiverPreferences>();

            mStagingPath = archivePreferences.StagingDirectory;
            mArchivePath = archivePreferences.ArchiveRootPath;
            mArchiveLabelFormat = archivePreferences.ArchiveLabelFormat;
            mArchiveLabelIdentifier = archivePreferences.ArchiveLabelIdentifier;

            FormatArchiveLabel();
            UpdateStagingDirectory();
            UpdateBurnDirectory();
        }

        private void UpdateStagingDirectory() {
            StagingList.Clear();
            StagingCount = 0;
            StagingSize = 0;

            RaisePropertyChanged(() => StagingCount);
            RaisePropertyChanged(() => StagingSize);

            ClearSelectedSet();

            StagingList.AddRange(mDirectoryScanner.ScanStagingDirectory());
            StagingCount = StagingList.Count;
            StagingSize = StagingList.Sum( album => album.Size );

            RaisePropertyChanged(() => StagingCount );
            RaisePropertyChanged(() => StagingSize );
            RaiseCanExecuteChangedEvent( "CanExecute_SelectSet" );
        }

        private void UpdateBurnDirectory() {
            ArchiveList.Clear();
            ArchiveList.AddRange( mDirectoryScanner.ScanArchiveDirectory());
        }

        private void ClearSelectedSet() {
            SelectedList.Clear();
            SelectedCount = 0;
            SelectedSize = 0;

            RaisePropertyChanged(() => StagingCount );
            RaisePropertyChanged(() => StagingSize );
            RaiseCanExecuteChangedEvent( "CanExecute_CreateArchive" );
        }

        public string StagingPath {
            get => mStagingPath;
            set {
                mStagingPath = value;

                var preferences = mPreferences.Load<ArchiverPreferences>();

                preferences.StagingDirectory = mStagingPath;
                mPreferences.Save( preferences );

                RaisePropertyChanged( () => StagingPath );
            }
        }

        public string ArchivePath {
            get => mArchivePath;
            set {
                mArchivePath = value;

                var preferences = mPreferences.Load<ArchiverPreferences>();

                preferences.ArchiveRootPath = mArchivePath;
                mPreferences.Save( preferences );

                RaisePropertyChanged( () => ArchivePath );
            }
        }

        public string ArchiveLabelFormat {
            get => mArchiveLabelFormat;
            set {
                mArchiveLabelFormat = value;

                var preferences = mPreferences.Load<ArchiverPreferences>();

                preferences.ArchiveLabelFormat = mArchiveLabelFormat;
                mPreferences.Save( preferences );

                RaisePropertyChanged( () => ArchiveLabelFormat );
                FormatArchiveLabel();
            }
        }

        public string ArchiveLabelIdentifier {
            get => mArchiveLabelIdentifier;
            set {
                mArchiveLabelIdentifier = value;

                var preferences = mPreferences.Load<ArchiverPreferences>();

                preferences.ArchiveLabelIdentifier = mArchiveLabelIdentifier;
                mPreferences.Save( preferences );

                RaisePropertyChanged( () => ArchiveLabelIdentifier );
                FormatArchiveLabel();
            }
        }

        private void FormatArchiveLabel() {
            ArchiveLabel = ArchiveLabelFormat.Replace( "{#}", ArchiveLabelIdentifier );

            RaisePropertyChanged( () => ArchiveLabel );
        }

        public void Execute_ScanDirectory() {
            UpdateStagingDirectory();
        }

        [DependsUpon( "StagingPath" )]
        public bool CanExecute_ScanDirectory() {
            return !string.IsNullOrWhiteSpace( StagingPath ) && Directory.Exists( StagingPath );
        }

        public void Execute_SelectSet() {
            ClearSelectedSet();

            SelectedList.AddRange( mSetCreator.GetBestAlbumSet( StagingList ));
            SelectedCount = SelectedList.Count;
            SelectedSize = SelectedList.Sum( album => album.Size );

            RaisePropertyChanged(() => SelectedCount);
            RaisePropertyChanged(() => SelectedSize);
        }

        public bool CanExecute_SelectSet() {
            return StagingList.Any();
        }

        public void Execute_CreateArchive() {
            mArchiveBuilder.ArchiveAlbums( SelectedList, ArchiveLabel );

            ClearSelectedSet();
            UpdateStagingDirectory();
            UpdateBurnDirectory();
        }

        [DependsUpon( "ArchivePath" )]
        [DependsUpon( "ArchiveLabel" )]
        public bool CanExecute_CreateArchive() {
            return !string.IsNullOrWhiteSpace( ArchivePath ) && !String.IsNullOrWhiteSpace( ArchiveLabel ) && SelectedList.Any();
        }

        public void Execute_BrowseForStagingDirectory() {
            var path = StagingPath;

            if( mDialogService.SelectFolderDialog( "Select Staging Directory", ref path ).GetValueOrDefault( false )) {
                StagingPath = path;
            }
        }

        public void Execute_BrowseForBurnDirectory() {
            var path = ArchivePath;

            if( mDialogService.SelectFolderDialog( "Select Archive Directory", ref path ).GetValueOrDefault( false )) {
                ArchivePath = path;
            }
        }

        public void Execute_OpenStagingFolder() {
            if( Directory.Exists( StagingPath )) {
                try {
                    System.Diagnostics.Process.Start( StagingPath );
                }
                catch( Exception ex ) {
                    mLog.LogException( "OnLaunchRequest:Staging Directory", ex );
                }
            }
        }

        [DependsUpon( "StagingPath" )]
        public bool CanExecute_OpenStagingFolder() {
            return !String.IsNullOrWhiteSpace( StagingPath ) && Directory.Exists( StagingPath );
        }

        public void Execute_OpenArchiveFolder() {
            if( Directory.Exists( ArchivePath )) {
                try {
                    System.Diagnostics.Process.Start( ArchivePath );
                }
                catch( Exception ex ) {
                    mLog.LogException( "OnLaunchRequest:Archive Directory", ex );
                }
            }
        }

        [DependsUpon( "ArchivePath")]
        public bool CanExecute_OpenArchiveFolder() {
            return !String.IsNullOrWhiteSpace( ArchivePath ) && Directory.Exists( ArchivePath );
        }
    }
}
