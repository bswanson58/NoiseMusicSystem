using System.Collections.ObjectModel;
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

        public  ObservableCollection<Album>     StagingList { get; }
        private string                          mStagingPath;
        public  int                             StagingCount { get; private set; }
        public  long                            StagingSize { get; private set; }

        public  ObservableCollection<Album>     SelectedList { get; }
        public  int                             SelectedCount { get; private set; }
        public  long                            SelectedSize { get; private set; }

        public  ObservableCollection<Album>     ArchiveList { get; }
        private string                          mArchivePath;
        public  string                          ArchiveLabelFormat { get; set; }
        public  string                          ArchiveLabelIdentifier { get; set; }
        public  string                          ArchiveLabel { get; private set; }

        public ArchiveCreatorViewModel( IDirectoryScanner directoryScanner, ISetCreator setCreator, IArchiveBuilder archiveBuilder, IPreferences preferences, IPlatformDialogService dialogService ) {
            mDirectoryScanner = directoryScanner;
            mSetCreator = setCreator;
            mArchiveBuilder = archiveBuilder;
            mPreferences = preferences;
            mDialogService = dialogService;

            StagingList = new ObservableCollection<Album>();
            SelectedList = new ObservableCollection<Album>();
            ArchiveList = new ObservableCollection<Album>();

            ArchiveLabelFormat = "DVD_{#}";
            ArchiveLabelIdentifier = "1277";
            FormatArchiveLabel();

            var archivePreferences = mPreferences.Load<ArchiverPreferences>();

            mStagingPath = archivePreferences.StagingDirectory;
            mArchivePath = archivePreferences.ArchiveRootPath;

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

        private void FormatArchiveLabel() {
            ArchiveLabel = ArchiveLabelFormat.Replace( "{#}", ArchiveLabelIdentifier );

            RaisePropertyChanged( () => ArchiveLabel );
        }

        public string StagingPath {
            get => mStagingPath;
            set {
                mStagingPath = value;

                var preferences = mPreferences.Load<ArchiverPreferences>();

                preferences.StagingDirectory = mStagingPath;
                mPreferences.Save( preferences );

                RaisePropertyChanged( () => StagingPath );
                RaiseCanExecuteChangedEvent( "CanExecute_ScanDirectory" );
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
                RaiseCanExecuteChangedEvent( "CanExecuteCreateArchive" );
            }
        }

        public void Execute_ScanDirectory() {
            UpdateStagingDirectory();
        }

        public bool CanExecute_ScanDirectory() {
            return !string.IsNullOrWhiteSpace( StagingPath );
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

        public bool CanExecute_CreateArchive() {
            return !string.IsNullOrWhiteSpace( ArchivePath ) && SelectedList.Any();
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
    }
}
