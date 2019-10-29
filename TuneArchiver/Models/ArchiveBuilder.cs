using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TuneArchiver.Interfaces;
using Unity.Interception.Utilities;

namespace TuneArchiver.Models {
    public class ArchiveBuilderProgress {
        public  long    AlbumCount { get; }
        public  long    AlbumsCompleted { get; }
        public  string  CurrentAlbum { get; }

        public ArchiveBuilderProgress( long albumCount, long albumsCompleted, string currentAlbum ) {
            AlbumCount = albumCount;
            AlbumsCompleted = albumsCompleted;
            CurrentAlbum = currentAlbum;
        }
    }

    class ArchiveBuilder : IArchiveBuilder {
        private readonly IPreferences   mPreferences;
        private readonly IPlatformLog   mLog;

        public ArchiveBuilder( IPreferences preferences, IPlatformLog log ) {
            mPreferences = preferences;
            mLog = log;
        }

        Task IArchiveBuilder.ArchiveAlbums( IEnumerable<Album> albums, string archiveTitle, IProgress<ArchiveBuilderProgress> progressReporter, CancellationTokenSource cancellation ) {
            return Task.Run( () => ArchiveAlbums( albums, archiveTitle, progressReporter, cancellation ));
        }

        private void ArchiveAlbums( IEnumerable<Album> albums, string archiveTitle, IProgress<ArchiveBuilderProgress> progressReporter, CancellationTokenSource cancellation ) {
            var archiveRootPath = CreateArchiveRoot( archiveTitle );

            if(!String.IsNullOrWhiteSpace( archiveRootPath )) {
                CleanSourceDirectories( CopyAlbums( albums.ToList(), archiveRootPath, progressReporter, cancellation ));
            }
        }

        private IEnumerable<Album> CopyAlbums( IList<Album> albums, string archiveRootPath, IProgress<ArchiveBuilderProgress> progressReporter, CancellationTokenSource cancellation ) {
            var retValue = new List<Album>();
            var albumCount = albums.Count();
            var currentAlbum = 0;

            foreach( var album in albums ) {
                try {
                    var albumPath = Path.Combine( archiveRootPath, album.ArtistName, album.AlbumName );

                    if(!Directory.Exists( albumPath )) {
                        Directory.CreateDirectory( albumPath );

                        if(!CopyFilesRecursively( new DirectoryInfo( album.Path ), new DirectoryInfo( albumPath ), cancellation )) {
                            break;
                        }

                        retValue.Add( album );
                    }

                    if( cancellation.IsCancellationRequested ) {
                        break;
                    }

                    currentAlbum++;

                    progressReporter.Report( new ArchiveBuilderProgress( albumCount, currentAlbum, album.DisplayName ));
                }
                catch( Exception ex ) {
                    mLog.LogException( $"Copying album '{album.Path}' to archive.", ex );

                    break;
                }
            }

            return retValue;
        }

        private bool CopyFilesRecursively( DirectoryInfo source, DirectoryInfo target, CancellationTokenSource cancellation ) {
            var retValue = true;

            foreach( var directory in source.GetDirectories()) {
                if(!CopyFilesRecursively( directory, target.CreateSubdirectory( directory.Name ), cancellation )) {
                    retValue = false;

                    break;
                }
            }

            foreach( var file in source.GetFiles()) {
                try {
                    file.MoveTo( Path.Combine( target.FullName, file.Name ));
                }
                catch( Exception ex ) {
                    mLog.LogException( $"Copying file '{file.Name}' to '{target.FullName}'.", ex );

                    retValue = false;
                    break;
                }

                if( cancellation.IsCancellationRequested ) {
                    retValue = false;

                    break;
                }
            }

            return retValue;
        }

        private void CleanSourceDirectories( IEnumerable<Album>  albums ) {
            albums.ForEach( album => {
                var path = album.Path;

                try {
                    Directory.Delete( path, true );

                    // delete the artist directory if it is empty.
                    path = new DirectoryInfo( album.Path ).Parent?.FullName;

                    if((!string.IsNullOrWhiteSpace( path )) &&
                       (!Directory.EnumerateFileSystemEntries( path ).Any())) {
                        Directory.Delete( path );
                    }
                }
                catch( Exception ex ) {
                    mLog.LogException( $"Deleting original directory: '{path}'", ex );
                }
            });
        }

        private string CreateArchiveRoot( string archiveTitle ) {
            var preferences = mPreferences.Load<ArchiverPreferences>();
            var archivePath = Path.Combine( preferences.ArchiveRootPath, archiveTitle );

            try {
                if(!Directory.Exists( archivePath )) {
                    Directory.CreateDirectory( archivePath );
                }
            }
            catch( Exception ex ) {
                mLog.LogException( $"Creating archive root: '{archivePath}'", ex );

                archivePath = String.Empty;
            }

            return archivePath;
        }
    }
}
