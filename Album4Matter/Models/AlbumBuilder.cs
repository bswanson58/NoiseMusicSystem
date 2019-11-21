using System;
using System.Collections.Generic;
using System.IO;
using Album4Matter.Dto;
using Album4Matter.Interfaces;

namespace Album4Matter.Models {
    class AlbumBuilder : IAlbumBuilder {
        private readonly IPreferences   mPreferences;
        private readonly IPlatformLog   mLog;

        public AlbumBuilder( IPreferences preferences, IPlatformLog log ) {
            mPreferences = preferences;
            mLog = log;
        }

        public void BuildAlbum( TargetAlbumLayout layout ) {
            try {
                var appPreferences = mPreferences.Load<Album4MatterPreferences>();
                var albumDirectory = Path.Combine( appPreferences.TargetDirectory, layout.ArtistName, layout.AlbumName );

                if(!Directory.Exists( albumDirectory )) {
                    Directory.CreateDirectory( albumDirectory );
                }

                MoveVolume( layout.AlbumList.VolumeContents, albumDirectory );
            }
            catch( Exception ex ) {
                mLog.LogException( "AlbumBuilder:BuildAlbum", ex );
            }
        }

        public void MoveVolume( IEnumerable<SourceItem> source, string targetFolder ) {
            if(!Directory.Exists( targetFolder )) {
                Directory.CreateDirectory( targetFolder );
            }

            foreach( var item in source ) {
                if( item is SourceFile file ) {
                    var destinationPath = Path.Combine( targetFolder, file.Name );

                    try {
                        if( File.Exists( destinationPath )) {
                            File.SetAttributes( destinationPath, FileAttributes.Normal );
                            File.Delete( destinationPath );
                        }

                        File.Copy( file.FileName, destinationPath );
                    }
                    catch( Exception ex ) {
                        mLog.LogException( $"Moving file: '{destinationPath}'", ex );
                    }
                }
                else if( item is SourceFolder folder ) {
                    MoveVolume( folder.Children, Path.Combine( targetFolder, folder.Name ));
                }
            }
        }
    }
}
