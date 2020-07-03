using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Recls;
using TuneArchiver.Interfaces;
using TuneArchiver.Support;

namespace TuneArchiver.Models {
    class DirectoryScanner : IDirectoryScanner {
        private readonly IPreferences   mPreferences;
        private readonly IPlatformLog   mLog;

        public DirectoryScanner( IPreferences preferences, IPlatformLog log ) {
            mPreferences = preferences;
            mLog = log;
        }

        public Task<IEnumerable<Album>> ScanStagingDirectory() {
            var preferences = mPreferences.Load<ArchiverPreferences>();

            return Task.Run(() => ScanRootDirectory( preferences.StagingDirectory ));
        }

        public Task<IEnumerable<string>> ScanArchiveDirectory() {
            var preferences = mPreferences.Load<ArchiverPreferences>();

            return Task.Run( () => ScanDirectory( preferences.ArchiveRootPath ));
        }

        private IEnumerable<string> ScanDirectory( string path ) {
            var retValue = new List<string>();

            try {
                if ( Directory.Exists( path )) {
                    var directories = FileSearcher.Search( path, null, SearchOptions.Directories, 0 );

                    directories.ForEach( directory => retValue.Add( directory.File ));
                }
            } 
            catch( Exception ex ) {
                mLog.LogException( $"Scanning directory: '{path}'", ex );
            }

            return retValue;
        }

        private IEnumerable<Album> ScanRootDirectory( string rootPath ) {
            var retValue = new List<Album>();

            try {
                if ( Directory.Exists( rootPath )) {
                    var directories = FileSearcher.Search( rootPath, null, SearchOptions.Directories, 0 );

                    directories.ForEach(directory => ScanDirectory( retValue, directory.File, directory.Path ));
                }
            }
            catch( Exception ex ) {
                mLog.LogException( $"ScanRootDirectory: '{rootPath}'", ex );
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
