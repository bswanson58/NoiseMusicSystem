using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TuneRenamer.Dto;
using TuneRenamer.Interfaces;
using TuneRenamer.Platform;
using TuneRenamer.Support;

namespace TuneRenamer.Models {
    class SourceScanner : ISourceScanner {
        private readonly IPlatformLog   mLog;
        private readonly IPreferences   mPreferences;
        private readonly IFileTypes     mFileTypes;

        public SourceScanner( IFileTypes fileTypes, IPreferences preferences, IPlatformLog log ) {
            mFileTypes = fileTypes;
            mPreferences = preferences;
            mLog = log;
        }

        public Task<List<SourceItem>> CollectFolder( string rootPath, Action<SourceFile> onItemInspect, Action<SourceFolder> copyNames, Action<SourceFolder> copyTags ) {
            var appPreferences = mPreferences.Load<TuneRenamerPreferences>();

            return Task.Run( () => {
                var dirInfo = new DirectoryInfo( rootPath );
                var rootFolder = new SourceFolder( rootPath, copyNames, copyTags, dirInfo.GetDirectorySize());

                try {
                    if( Directory.Exists( rootPath )) {
                        CollectFolder( rootFolder, onItemInspect, copyNames, copyTags, appPreferences.SkipUnderscoreDirectories );
                    }
                }
                catch( Exception ex ) {
                    mLog.LogException( $"CollectFolder: '{rootPath}'", ex );
                }

                return rootFolder.Children.ToList();
            });
        }

        private void CollectFolder( SourceFolder rootFolder, Action<SourceFile> onItemInspect, Action<SourceFolder> copyNames, Action<SourceFolder> copyTags, bool skipUnderscoredDirectories ) {
            foreach( var directory in Directory.GetDirectories( rootFolder.FileName )) {
                var directoryName = Path.GetFileName( directory );

                if(!String.IsNullOrWhiteSpace( directoryName )) {
                    if(( skipUnderscoredDirectories ) &&
                       ( directoryName.StartsWith( "_" ))) {
                        continue;
                    }

                    var dirInfo = new DirectoryInfo( directory );
                    var folder = new SourceFolder( directory, copyNames, copyTags, dirInfo.GetDirectorySize());

                    rootFolder.Children.Add( folder );
                    CollectFolder( folder, onItemInspect, copyNames, copyTags, skipUnderscoredDirectories );
                }
            }

            foreach( var file in Directory.EnumerateFiles( rootFolder.FileName )) {
                rootFolder.Children.Add( new SourceFile( file, mFileTypes.ItemIsMusicFile( file ), mFileTypes.ItemIsTextFile( file ), onItemInspect ));
            }
        }

        public Task AddTags( IEnumerable<SourceItem> items ) {
            return Task.Run( () => {
                foreach( var item in items ) {
                    if( item is SourceFolder folder ) {
                        AddTags( folder.Children );
                    }
                    if(( item is SourceFile file ) &&
                       ( mFileTypes.ItemIsMusicFile( item ))) {
                       AddTags( file );
                    }
                }
            });
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
                mLog.LogMessage( $"LoadMusicTags from: '{file.FileName}' - error: {ex.Message}" );
            }
        }
    }
}
