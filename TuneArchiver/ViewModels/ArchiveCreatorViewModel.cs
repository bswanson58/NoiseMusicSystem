using System.Collections.Generic;
using System.Linq;
using ReusableBits.Mvvm.ViewModelSupport;
using TuneArchiver.Interfaces;
using TuneArchiver.Models;

namespace TuneArchiver.ViewModels {
    class ArchiveCreatorViewModel : AutomaticCommandBase {
        private readonly IDirectoryScanner  mDirectoryScanner;
        private readonly ISetCreator        mSetCreator;
        private readonly IArchiveBuilder    mArchiveBuilder;
        private readonly List<Album>        mDirectoryList;
        private readonly List<Album>        mSelectedSet;
        private long                        mSelectedSetSize;

        public ArchiveCreatorViewModel( IDirectoryScanner directoryScanner, ISetCreator setCreator, IArchiveBuilder archiveBuilder ) {
            mDirectoryScanner = directoryScanner;
            mSetCreator = setCreator;
            mArchiveBuilder = archiveBuilder;

            mDirectoryList = new List<Album>();
            mSelectedSet = new List<Album>();
        }

        public void Execute_ScanDirectory() {
            mDirectoryList.Clear();
            mSelectedSet.Clear();
            mSelectedSetSize = 0L;

            mDirectoryList.AddRange( mDirectoryScanner.ScanStagingArea());
            mSelectedSet.AddRange( mSetCreator.GetBestAlbumSet( mDirectoryList ));
            mSelectedSetSize = mSelectedSet.Sum( album => album.Size );
        }
    }
}
