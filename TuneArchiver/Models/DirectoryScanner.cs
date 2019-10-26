using System.Collections.Generic;
using System.IO;
using Recls;
using TuneArchiver.Interfaces;
using TuneArchiver.Support;

namespace TuneArchiver.Models {
    class DirectoryScanner : IDirectoryScanner {

        public IEnumerable<Album> ScanStagingArea() {
            return ScanStagingDirectory( @"D:\Music" );
        }

        private IEnumerable<Album> ScanStagingDirectory( string rootPath ) {
            var retValue = new List<Album>();
            var directories = FileSearcher.Search( rootPath, null, SearchOptions.Directories, 0 );

            directories.ForEach( directory => ScanDirectory( retValue, directory.FileName, directory.Path ));

            return retValue;
        }

        private void ScanDirectory( IList<Album> directoryList, string name, string path ) {
            var directories = FileSearcher.Search( path, null, SearchOptions.Directories, 0 );

            directories.ForEach( directory => { directoryList.Add( CollectDirectory( name, directory.FileName, directory.Path )); });
        }

        private Album CollectDirectory( string artist, string album, string path ) {
            var dirInfo = new DirectoryInfo( path );

            return new Album( artist, album, path, dirInfo.GetDirectorySize());
        }
    }
   
}
