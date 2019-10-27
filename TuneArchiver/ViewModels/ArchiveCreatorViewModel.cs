using System.Collections.ObjectModel;
using System.Linq;
using ReusableBits.Mvvm.ViewModelSupport;
using TuneArchiver.Interfaces;
using TuneArchiver.Models;

namespace TuneArchiver.ViewModels {
    class ArchiveCreatorViewModel : AutomaticCommandBase {
        private readonly IDirectoryScanner  mDirectoryScanner;
        private readonly ISetCreator        mSetCreator;
        private readonly IArchiveBuilder    mArchiveBuilder;
        private readonly IPreferences       mPreferences;

        public  ObservableCollection<Album> StagingList { get; }
        public  string                      StagingPath {get; set; }
        public  int                         StagingCount { get; private set; }
        public  long                        StagingSize { get; private set; }

        public  ObservableCollection<Album> SelectedList { get; }
        public  int                         SelectedCount { get; private set; }
        public  long                        SelectedSize { get; private set; }

        public  ObservableCollection<Album> ArchiveList { get; }
        public  string                      ArchivePath { get; set; }
        public  string                      ArchiveLabelFormat { get; set; }
        public  string                      ArchiveLabelIdentifier { get; set; }
        public  string                      ArchiveLabel { get; private set; }

        public ArchiveCreatorViewModel( IDirectoryScanner directoryScanner, ISetCreator setCreator, IArchiveBuilder archiveBuilder, IPreferences preferences ) {
            mDirectoryScanner = directoryScanner;
            mSetCreator = setCreator;
            mArchiveBuilder = archiveBuilder;
            mPreferences = preferences;

            StagingList = new ObservableCollection<Album>();
            SelectedList = new ObservableCollection<Album>();
            ArchiveList = new ObservableCollection<Album>();

            ArchiveLabelFormat = "DVD_{#}";
            ArchiveLabelIdentifier = "1277";
            FormatArchiveLabel();

            var archivePreferences = mPreferences.Load<ArchiverPreferences>();

            archivePreferences.StagingDirectory = @"D:\Music";
            archivePreferences.ArchiveRootPath = @"D:\Burn";

            mPreferences.Save( archivePreferences );

            StagingPath = archivePreferences.StagingDirectory;
            ArchivePath = archivePreferences.ArchiveRootPath;

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
            StagingSize = StagingList.Sum(album => album.Size);

            RaisePropertyChanged(() => StagingCount);
            RaisePropertyChanged(() => StagingSize);
        }

        private void UpdateBurnDirectory() {
            ArchiveList.Clear();
            ArchiveList.AddRange( mDirectoryScanner.ScanArchiveDirectory());
        }

        private void ClearSelectedSet() {
            SelectedList.Clear();
            SelectedCount = 0;
            SelectedSize = 0;

            RaisePropertyChanged(() => StagingCount);
            RaisePropertyChanged(() => StagingSize);
        }

        private void FormatArchiveLabel() {
            ArchiveLabel = ArchiveLabelFormat.Replace( "{#}", ArchiveLabelIdentifier );

            RaisePropertyChanged( () => ArchiveLabel );
        }

        public void Execute_ScanDirectory() {
            UpdateStagingDirectory();
        }

        public void Execute_SelectSet() {
            ClearSelectedSet();

            SelectedList.AddRange( mSetCreator.GetBestAlbumSet( StagingList ));
            SelectedCount = SelectedList.Count;
            SelectedSize = SelectedList.Sum( album => album.Size );

            RaisePropertyChanged(() => SelectedCount);
            RaisePropertyChanged(() => SelectedSize);
        }

        public void Execute_CreateArchive() {
            mArchiveBuilder.ArchiveAlbums( SelectedList, ArchiveLabel );

            ClearSelectedSet();
            UpdateStagingDirectory();
            UpdateBurnDirectory();
        }
    }
}
