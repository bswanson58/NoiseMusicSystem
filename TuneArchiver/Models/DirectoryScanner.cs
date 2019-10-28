using System.Collections.Generic;
using System.IO;
using Recls;
using TuneArchiver.Interfaces;
using TuneArchiver.Support;

namespace TuneArchiver.Models {
    class DirectoryScanner : IDirectoryScanner {
        private readonly IPreferences   mPreferences;

        public DirectoryScanner( IPreferences preferences ) {
            mPreferences = preferences;
        }

        public IEnumerable<Album> ScanStagingDirectory() {
            var preferences = mPreferences.Load<ArchiverPreferences>();

            return ScanRootDirectory( preferences.StagingDirectory );
        }

        public IEnumerable<string> ScanArchiveDirectory() {
            var retValue = new List<string>();
            var preferences = mPreferences.Load<ArchiverPreferences>();

            if ( Directory.Exists( preferences.ArchiveRootPath )) {
                var directories = FileSearcher.Search( preferences.ArchiveRootPath, null, SearchOptions.Directories, 0 );

                directories.ForEach( directory => retValue.Add( directory.File ));
            }

            return retValue;
        }

        private IEnumerable<Album> ScanRootDirectory( string rootPath ) {
            var retValue = new List<Album>();

            if ( Directory.Exists( rootPath )) {
                var directories = FileSearcher.Search( rootPath, null, SearchOptions.Directories, 0 );

                directories.ForEach(directory => ScanDirectory( retValue, directory.File, directory.Path ));
            }

            return retValue;
        }

        private void ScanDirectory( IList<Album> directoryList, string name, string path ) {
            var directories = FileSearcher.Search( path, null, SearchOptions.Directories, 0 );

            directories.ForEach( directory => { directoryList.Add( CollectDirectory( name, directory.File, directory.Path ));
            });
        }

        private Album CollectDirectory( string artist, string album, string path ) {
            var dirInfo = new DirectoryInfo( path );

            return new Album( artist, album, path, dirInfo.GetDirectorySize());
        }
    }
   
}
