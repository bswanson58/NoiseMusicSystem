using System;
using System.Collections.Generic;
using System.IO;
using Album4Matter.Dto;
using Album4Matter.Interfaces;
using Album4Matter.Platform;

namespace Album4Matter.Models {
    class SourceScanner : ISourceScanner {
        private readonly IPlatformLog   mLog;

        public SourceScanner( IPlatformLog log ) {
            mLog = log;
        }

        public IEnumerable<SourceItem> CollectFolder( string rootPath, Action<SourceItem> onItemInspect ) {
            var rootFolder = new SourceFolder( rootPath, null );

            if( Directory.Exists( rootPath )) {
                CollectFolder( rootFolder, onItemInspect );
            }

            return rootFolder.Children;
        }

        private void CollectFolder( SourceFolder rootFolder, Action<SourceItem> onItemInspect ) {
            foreach( var directory in Directory.GetDirectories( rootFolder.FileName )) {
                var folder = new SourceFolder( directory, rootFolder.Key, onItemInspect );

                rootFolder.Children.Add( folder );
                CollectFolder( folder, onItemInspect );
            }

            foreach( var file in Directory.EnumerateFiles( rootFolder.FileName )) {
                rootFolder.Children.Add( new SourceFile( file, rootFolder.Key, onItemInspect ));
            }
        }

        public void AddTags( IEnumerable<SourceItem> items ) {
            foreach( var item in items ) {
                if( item is SourceFolder folder ) {
                    AddTags( folder.Children );
                }
                if( item is SourceFile file ) {
                    AddTags( file );
                }
            }
        }

        private void AddTags( SourceFile file ) {
            try {
                var tags = TagLib.File.Create( file.FileName );
                var artist = String.Empty;
                var album = String.Empty;
                var track = 0;
                var title = String.Empty;

                if(!String.IsNullOrWhiteSpace( tags.Tag.JoinedPerformers )) {
                    artist = string.Join(", ", tags.Tag.JoinedPerformers).Trim();
                }
                if(!String.IsNullOrWhiteSpace( tags.Tag.Album )) {
                    album = tags.Tag.Album.Trim();
                }
                if(!String.IsNullOrWhiteSpace( tags.Tag.Title )) {
                    title = tags.Tag.Title.Trim();
                }
                if( tags.Tag.Track > 0 ) {
                    track = (int)tags.Tag.Track;
                }

                if(( track > 0 ) &&
                   (!String.IsNullOrWhiteSpace( title ))) {
                    var extension = Path.GetExtension( file.Name );

                    if(!String.IsNullOrWhiteSpace( extension )) {
                        var fileName = PathSanitizer.SanitizeFilename( $"{track:D2} - {title}{extension}", ' ' ).Trim();

                        file.SetTags( artist, album, track, title, fileName );
                    }
                }
            }
            catch( Exception ex ) {
                mLog.LogException( $"LoadMusicTags from: '{file.FileName}'", ex );
            }
        }
    }
}
