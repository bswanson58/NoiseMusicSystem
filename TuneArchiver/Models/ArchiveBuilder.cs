using System.Collections.Generic;
using System.IO;
using TuneArchiver.Interfaces;
using Unity.Interception.Utilities;

namespace TuneArchiver.Models {
    class ArchiveBuilder : IArchiveBuilder {
        private readonly IPreferences   mPreferences;

        public ArchiveBuilder( IPreferences preferences ) {
            mPreferences = preferences;
        }

        public void ArchiveAlbums( IEnumerable<Album> albums, string archiveTitle ) {
            var archiveRootPath = CreateArchiveRoot( archiveTitle );

            CopyAlbums( albums, archiveRootPath );
        }

        private void CopyAlbums( IEnumerable<Album>  albums, string archiveRootPath ) {
            albums.ForEach( album => {
                var albumPath = Path.Combine( archiveRootPath, album.ArtistName, album.AlbumName );

                if(!Directory.Exists( albumPath )) {
                    Directory.CreateDirectory( albumPath );

                    CopyFilesRecursively( new DirectoryInfo( album.Path ), new DirectoryInfo( albumPath ));
                }
            });
        }

        public static void CopyFilesRecursively( DirectoryInfo source, DirectoryInfo target ) {
            foreach( var directory in source.GetDirectories()) {
                CopyFilesRecursively( directory, target.CreateSubdirectory( directory.Name ));
            }

            foreach( var file in source.GetFiles()) {
                file.CopyTo( Path.Combine( target.FullName, file.Name ));
            }
        }

        private string CreateArchiveRoot( string archiveTitle ) {
            var preferences = mPreferences.Load<ArchiverPreferences>();
            var archivePath = Path.Combine( preferences.ArchiveRootPath, archiveTitle );

            if(!Directory.Exists( archivePath )) {
                Directory.CreateDirectory( archivePath );
            }

            return archivePath;
        }
    }
}
