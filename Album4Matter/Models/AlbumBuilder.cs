using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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

        public Task<bool> BuildAlbum( TargetAlbumLayout layout ) {
            return Task.Run( () => {
                var retValue = true;

                try {
                    var appPreferences = mPreferences.Load<Album4MatterPreferences>();
                    var albumDirectory = Path.Combine( appPreferences.TargetDirectory, layout.ArtistName, layout.AlbumName );

                    if(!Directory.Exists( albumDirectory )) {
                        Directory.CreateDirectory( albumDirectory );
                    }

                    retValue = MoveVolume( layout.AlbumList.VolumeContents, albumDirectory );
                }
                catch( Exception ex ) {
                    mLog.LogException( "AlbumBuilder:BuildAlbum", ex );

                    retValue = false;
                }

                return retValue;
            });
        }

        public bool MoveVolume( IEnumerable<SourceItem> source, string targetFolder ) {
            var retValue = true;

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

                        retValue = false;
                    }
                }
                else if( item is SourceFolder folder ) {
                    retValue = MoveVolume( folder.Children, Path.Combine( targetFolder, folder.Name ));
                }
            }

            return retValue;
        }
    }
}
