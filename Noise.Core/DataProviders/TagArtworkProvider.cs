using System;
using System.Linq;
using System.Text.RegularExpressions;
using Noise.Infrastructure.Interfaces;
using File = System.IO.File;
using TagFile = TagLib.File;

namespace Noise.Core.DataProviders {
    class TagArtworkProvider : ITagArtworkProvider {
        private readonly IStorageFileProvider   mFileProvider;
        private readonly IStorageFolderSupport  mFolderSupport;
        private readonly INoiseLog              mLog;

        public TagArtworkProvider( IStorageFileProvider fileProvider, IStorageFolderSupport folderSupport, INoiseLog log ) {
            mFileProvider = fileProvider;
            mFolderSupport = folderSupport;
            mLog = log;
        }

        public byte[] GetArtwork( long storageFileId, string name ) {
            var retValue = new Byte[0];

            try {
                var storageFile = mFileProvider.GetFile( storageFileId );
                var path = mFolderSupport.GetPath( storageFile );

                if( File.Exists( path )) {
                    var tagFile = OpenTagFile( path );
                    var pictures = tagFile?.Tag.Pictures;

                    if(( pictures != null ) && 
                       ( pictures.Any())) {
                        var index = GetPictureIndex( name );
                        var picture = pictures.Skip( index ).Take( 1 ).FirstOrDefault();

                        if( picture != null ) {
                            retValue = picture.Data.ToArray();
                        }
                    }
                }
            }
            catch( Exception ex ) {
                mLog.LogException( $"Can't retrieve artwork named '{name}' from tag for file id: {storageFileId}", ex );
            }

            return retValue;
        }

        private TagFile OpenTagFile( string path ) {
            TagFile retValue = null;

            try {
                retValue = TagFile.Create( path );
            }
            catch( Exception ex ) {
                mLog.LogException( $"Opening file \"{path}\"", ex );
            }

            return( retValue );			
        }

        private int GetPictureIndex( string name ) {
            var retValue = 0;
            var pattern = ".*?(\\d+)";
            var regex = new Regex( pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline );
            var match = regex.Match( name );

            if( match.Success ) {
                retValue = Int32.Parse( match.Groups[1].Value );
            }

            return retValue;
        }
    }
}
