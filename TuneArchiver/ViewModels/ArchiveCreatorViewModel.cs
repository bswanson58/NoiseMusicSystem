using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;
using TuneArchiver.Interfaces;
using TuneArchiver.Models;
using TuneArchiver.Platform;

namespace TuneArchiver.ViewModels {
    class ArchiveCreatorViewModel : PropertyChangeBase {
        private readonly IDirectoryScanner      mDirectoryScanner;
        private readonly ISetCreator            mSetCreator;
        private readonly IArchiveBuilder        mArchiveBuilder;
        private readonly IPreferences           mPreferences;
        private readonly IPlatformDialogService mDialogService;
        private readonly IPlatformLog           mLog;
        private readonly IArchiveMedia          mArchiveMedia;

        public  ObservableCollection<Album>     StagingList { get; }
        private string                          mStagingPath;
        public  int                             StagingCount { get; private set; }
        public  long                            StagingSize { get; private set; }

        public  ObservableCollection<Album>     SelectedList { get; }
        public  int                             SelectedCount { get; private set; }
        public  long                            SelectedSize { get; private set; }
        public  bool                            CreatingSet { get; private set; }
        public  long                            ArchiveSetSize {  get; private set; }
        public  bool                            SelectionLevelLow => ( SelectedSize > 0 ) && ((float)SelectedSize / ArchiveSetSize ) < 0.99;
        public  bool                            SelectionLevelAdequate => ( SelectedSize > 0 ) && ((float)SelectedSize / ArchiveSetSize ) > 0.99;
        private CancellationTokenSource         mSetCreationCancellation;

        public  ObservableCollection<string>    ArchiveList { get; }
        private string                          mArchivePath;
        private string                          mArchiveLabel;
        private string                          mArchiveLabelFormat;
        private string                          mArchiveLabelIdentifier;

        public  bool                            CreatingArchive {  get; private set; }
        public  long                            ArchiveAlbumCount { get; private set; }
        public  long                            ArchiveAlbumsCompleted { get; set; }
        private CancellationTokenSource         mArchiveBuilderCancellation;

        public  IList<ArchiveMedia>             MediaList => mArchiveMedia.MediaTypes;
        private ArchiveMedia                    mSelectedMedia;

        public  DelegateCommand                 ScanDirectory { get; }
        public  DelegateCommand                 SelectSet { get; }
        public  DelegateCommand                 CancelSetCreation { get; }
        public  DelegateCommand                 CreateArchive { get; }
        public  DelegateCommand                 CancelArchiveBuilding { get; }
        public  DelegateCommand                 BrowseForBurnDirectory { get; }
        public  DelegateCommand                 BrowseForStagingDirectory { get; }
        public  DelegateCommand                 OpenArchiveFolder { get; }
        public  DelegateCommand                 OpenStagingFolder { get; }


        public ArchiveCreatorViewModel( IDirectoryScanner directoryScanner, ISetCreator setCreator, IArchiveBuilder archiveBuilder, IPreferences preferences, 
                                        IPlatformDialogService dialogService, IPlatformLog log, IArchiveMedia archiveMedia ) {
            mDirectoryScanner = directoryScanner;
            mSetCreator = setCreator;
            mArchiveBuilder = archiveBuilder;
            mPreferences = preferences;
            mDialogService = dialogService;
            mLog = log;
            mArchiveMedia = archiveMedia;

            StagingList = new ObservableCollection<Album>();
            SelectedList = new ObservableCollection<Album>();
            ArchiveList = new ObservableCollection<string>();

            ScanDirectory = new DelegateCommand( OnScanDirectory, CanScanDirectory );
            SelectSet = new DelegateCommand( OnSelectSet, CanSelectSet );
            CancelSetCreation = new DelegateCommand( OnCancelSetCreation );
            CreateArchive = new DelegateCommand( OnCreateArchive, CanCreateArchive );
            CancelArchiveBuilding = new DelegateCommand( OnCancelArchiveBuilding );
            BrowseForBurnDirectory = new DelegateCommand( OnBrowseForBurnDirectory );
            BrowseForStagingDirectory = new DelegateCommand( OnBrowseForStagingDirectory );
            OpenArchiveFolder = new DelegateCommand( OnOpenArchiveFolder, CanOpenArchiveFolder );
            OpenStagingFolder = new DelegateCommand( OnOpenStagingFolder, CanOpenStagingFolder );

            var archivePreferences = mPreferences.Load<ArchiverPreferences>();

            mStagingPath = archivePreferences.StagingDirectory;
            mArchivePath = archivePreferences.ArchiveRootPath;
            mArchiveLabelFormat = archivePreferences.ArchiveLabelFormat;
            mArchiveLabelIdentifier = archivePreferences.ArchiveLabelIdentifier;
            mSelectedMedia = MediaList.FirstOrDefault( media => media.Name.Equals( archivePreferences.ArchiveMediaType )) ??
                             MediaList.FirstOrDefault();

            FormatArchiveLabel();
            UpdateStagingDirectory();
            UpdateBurnDirectory();
        }

        private async void UpdateStagingDirectory() {
            StagingList.Clear();
            StagingCount = 0;
            StagingSize = 0;

            RaisePropertyChanged(() => StagingCount);
            RaisePropertyChanged(() => StagingSize);

            ClearSelectedSet();

            var stagingList = await mDirectoryScanner.ScanStagingDirectory();
            StagingList.AddRange( stagingList.OrderBy( a => a.DisplayName ));
            StagingCount = StagingList.Count;
            StagingSize = StagingList.Sum( album => album.Size );

            RaisePropertyChanged(() => StagingCount );
            RaisePropertyChanged(() => StagingSize );
            SelectSet.RaiseCanExecuteChanged();
        }

        private async void UpdateBurnDirectory() {
            ArchiveList.Clear();

            var directoryList = await mDirectoryScanner.ScanArchiveDirectory();
            ArchiveList.AddRange( directoryList.OrderBy( d => d ));
        }

        private void ClearSelectedSet() {
            SelectedList.Clear();
            SelectedCount = 0;
            SelectedSize = 0;

            RaisePropertyChanged(() => StagingCount );
            RaisePropertyChanged(() => StagingSize );
            RaisePropertyChanged( () => SelectedSize );
            RaisePropertyChanged( () => SelectedCount );
            RaisePropertyChanged( () => SelectionLevelAdequate );
            RaisePropertyChanged( () => SelectionLevelLow );
            CreateArchive.RaiseCanExecuteChanged();
        }

        public string StagingPath {
            get => mStagingPath;
            set {
                mStagingPath = value;

                var preferences = mPreferences.Load<ArchiverPreferences>();

                preferences.StagingDirectory = mStagingPath;
                mPreferences.Save( preferences );

                RaisePropertyChanged( () => StagingPath );
                ScanDirectory.RaiseCanExecuteChanged();
                OpenStagingFolder.RaiseCanExecuteChanged();
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
                OpenArchiveFolder.RaiseCanExecuteChanged();
                CreateArchive.RaiseCanExecuteChanged();
            }
        }

        public string ArchiveLabel {
            get => mArchiveLabel;
            set {
                mArchiveLabel = value;

                RaisePropertyChanged( () => ArchiveLabel );
                CreateArchive.RaiseCanExecuteChanged();
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

        public ArchiveMedia SelectedMedia {
            get => mSelectedMedia;
            set {
                mSelectedMedia = value;

                var preferences = mPreferences.Load<ArchiverPreferences>();

                preferences.ArchiveMediaType = mSelectedMedia.Name;
                mPreferences.Save( preferences );

                SelectSet.RaiseCanExecuteChanged();
            }
        }

        private void UpdateArchiveIdentifier() {
            if( Int32.TryParse( ArchiveLabelIdentifier, out var identifier )) {
                identifier++;

                ArchiveLabelIdentifier = identifier.ToString();
            }
        }

        private void OnScanDirectory() {
            UpdateStagingDirectory();
        }

        public bool CanScanDirectory() {
            return !string.IsNullOrWhiteSpace( StagingPath ) && Directory.Exists( StagingPath );
        }

        private async void OnSelectSet() {
            ClearSelectedSet();

            var progressReporter = new Progress<SetCreatorProgress>();
            progressReporter.ProgressChanged += OnSetCreatorProgress;

            mSetCreationCancellation = new CancellationTokenSource();

            CreatingSet = true;
            RaisePropertyChanged( () => CreatingSet );

            var selectedList = await mSetCreator.GetBestAlbumSet( StagingList, progressReporter, mSetCreationCancellation );

            SelectedList.AddRange( selectedList.OrderBy( a => a.DisplayName ));
            SelectedCount = SelectedList.Count;
            SelectedSize = SelectedList.Sum( album => album.Size );

            CreatingSet = false;

            RaisePropertyChanged( () => CreatingSet );
            RaisePropertyChanged( () => SelectedCount);
            RaisePropertyChanged( () => SelectedSize);
            RaisePropertyChanged( () => SelectionLevelLow );
            RaisePropertyChanged( () => SelectionLevelAdequate );
            SelectSet.RaiseCanExecuteChanged();
            CreateArchive.RaiseCanExecuteChanged();
        }

        private bool CanSelectSet() {
            return StagingList.Any() && !CreatingSet && ( mSelectedMedia != null );
        }

        private void OnCancelSetCreation() {
            mSetCreationCancellation?.Cancel();
        }

        private void OnSetCreatorProgress( object sender, SetCreatorProgress progress ) {
            ArchiveSetSize = progress.ArchiveSize;
            SelectedSize = progress.CurrentSetSize;

            RaisePropertyChanged( () => ArchiveSetSize );
            RaisePropertyChanged( () => SelectedSize );
            RaisePropertyChanged( () => SelectionLevelLow );
            RaisePropertyChanged( () => SelectionLevelAdequate );
        }

        private async void OnCreateArchive() {
            CreatingArchive = true;
            RaisePropertyChanged( () => CreatingArchive );

            var progressReporter = new Progress<ArchiveBuilderProgress>();
            progressReporter.ProgressChanged += OnArchiveBuilderProgress;

            mArchiveBuilderCancellation = new CancellationTokenSource();

            await mArchiveBuilder.ArchiveAlbums( SelectedList, ArchiveLabel, progressReporter, mArchiveBuilderCancellation );

            ClearSelectedSet();
            UpdateStagingDirectory();
            UpdateBurnDirectory();
            UpdateArchiveIdentifier();

            CreatingArchive = false;
            RaisePropertyChanged( () => CreatingArchive );
        }

        private bool CanCreateArchive() {
            return !string.IsNullOrWhiteSpace( ArchivePath ) && !String.IsNullOrWhiteSpace( ArchiveLabel ) && SelectedList.Any();
        }

        private void OnArchiveBuilderProgress( object sender, ArchiveBuilderProgress progress ) {
            ArchiveAlbumCount = progress.AlbumCount;
            ArchiveAlbumsCompleted = Math.Min( ArchiveAlbumsCompleted, ArchiveAlbumCount );

            RaisePropertyChanged( () => ArchiveAlbumCount );
            RaisePropertyChanged( () => ArchiveAlbumsCompleted );
        }

        private void OnCancelArchiveBuilding() {
            mArchiveBuilderCancellation?.Cancel();
        }

        private void OnBrowseForStagingDirectory() {
            var path = StagingPath;

            if( mDialogService.SelectFolderDialog( "Select Staging Directory", ref path ).GetValueOrDefault( false )) {
                StagingPath = path;
            }
        }

        private void OnBrowseForBurnDirectory() {
            var path = ArchivePath;

            if( mDialogService.SelectFolderDialog( "Select Archive Directory", ref path ).GetValueOrDefault( false )) {
                ArchivePath = path;
            }
        }

        private void OnOpenStagingFolder() {
            if( Directory.Exists( StagingPath )) {
                try {
                    System.Diagnostics.Process.Start( StagingPath );
                }
                catch( Exception ex ) {
                    mLog.LogException( "OnLaunchRequest:Staging Directory", ex );
                }
            }
        }

        private bool CanOpenStagingFolder() {
            return !String.IsNullOrWhiteSpace( StagingPath ) && Directory.Exists( StagingPath );
        }

        private void OnOpenArchiveFolder() {
            if( Directory.Exists( ArchivePath )) {
                try {
                    System.Diagnostics.Process.Start( ArchivePath );
                }
                catch( Exception ex ) {
                    mLog.LogException( "OnLaunchRequest:Archive Directory", ex );
                }
            }
        }

        private bool CanOpenArchiveFolder() {
            return !String.IsNullOrWhiteSpace( ArchivePath ) && Directory.Exists( ArchivePath );
        }
    }
}
