using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                bool retValue;

                try {
                    var appPreferences = mPreferences.Load<Album4MatterPreferences>();
                    var albumDirectory = Path.Combine( appPreferences.TargetDirectory, layout.ArtistName, layout.AlbumName );

                    if(!Directory.Exists( albumDirectory )) {
                        Directory.CreateDirectory( albumDirectory );
                    }

                    retValue = MoveVolume( layout.AlbumList.VolumeContents, albumDirectory );

                    foreach( var volume in layout.VolumeList ) {
                        var volumeDirectory = Path.Combine( albumDirectory, volume.VolumeName );

                        if(!Directory.Exists( volumeDirectory )) {
                            Directory.CreateDirectory( volumeDirectory );
                        }

                        retValue &= MoveVolume( volume.VolumeContents, volumeDirectory );
                    }
                }
                catch( Exception ex ) {
                    mLog.LogException( "AlbumBuilder:BuildAlbum", ex );

                    retValue = false;
                }

                return retValue;
            });
        }

        private bool MoveVolume( IEnumerable<SourceItem> source, string targetFolder ) {
            var retValue = true;

            if(!Directory.Exists( targetFolder )) {
                Directory.CreateDirectory( targetFolder );
            }

            foreach( var item in source ) {
                if( item is SourceFile file ) {
                    var fileName = file.HasTagName && file.UseTagNameAsTarget ? file.TagName : file.Name;
                    var destinationPath = Path.Combine( targetFolder, fileName );

                    try {
                        if( File.Exists( destinationPath )) {
                            File.SetAttributes( destinationPath, FileAttributes.Normal );
                            File.Delete( destinationPath );
                        }

                        File.Move( file.FileName, destinationPath );
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

        public Task<bool> ClearSourceDirectory( TargetAlbumLayout layout ) {
            return Task.Run( () => {
                var retValue = false;

                try {
                    var pathList = new List<string>();

                    pathList.AddRange( CollectPaths( layout.AlbumList.VolumeContents ));

                    foreach( var volume in layout.VolumeList ) {
                        pathList.AddRange( CollectPaths( volume.VolumeContents ));
                    }

                    var rootDirectory = pathList.OrderByDescending( p => p.Length ).LastOrDefault();

                    if((!String.IsNullOrWhiteSpace( rootDirectory )) &&
                       (!DirectoryHasFiles( rootDirectory ))) {
                        Directory.Delete( rootDirectory, true );

                        retValue = true;
                    }
                }
                catch( Exception ex ) {
                    mLog.LogException( "ClearSourceDirectory", ex );
                }

                return retValue;
            });
        }

        private IEnumerable<string> CollectPaths( IEnumerable<SourceItem> list ) {
            var retValue = new List<string>();

            foreach( var item in list ) {
                if( item is SourceFile file ) {
                    retValue.Add( Path.GetDirectoryName( file.FileName ));
                }
                else if( item is SourceFolder folder ) {
                    retValue.Add( folder.FileName );
                }
            }

            return retValue;
        }

        private bool DirectoryHasFiles( string rootDirectory ) {
            return Directory.GetFiles( rootDirectory, "*", SearchOption.AllDirectories ).Any();
        }
    }
}
