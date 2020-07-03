using System.Globalization;
using System.IO;
using System.Linq;
using TuneRenamer.Dto;
using TuneRenamer.Interfaces;

namespace TuneRenamer.Platform {
    public class FileTypes : IFileTypes {
        private readonly string[]   mTextFileExtensions = { ".txt", ".nfo" };
        private readonly string[]   mMusicFileExtensions = { ".mp3", ".flac" };

        public bool ItemIsTextFile( SourceItem item ) {
            var retValue = false;

            if( item is SourceFile ) {
                retValue = ItemIsTextFile( item.FileName );
            }

            return retValue;
        }

        public bool ItemIsTextFile( string fileName ) {
            return mTextFileExtensions.Any( e => e.Equals( Path.GetExtension( fileName )?.ToLower( CultureInfo.CurrentUICulture )));
        }

        public bool ItemIsMusicFile( SourceItem item ) {
            var retValue = false;

            if( item is SourceFile ) {
                retValue = ItemIsMusicFile( item.FileName );
            }

            return retValue;
        }

        public bool ItemIsMusicFile( string fileName ) {
            return mMusicFileExtensions.Any( e => e.Equals( Path.GetExtension( fileName )?.ToLower( CultureInfo.CurrentUICulture )));
        }
    }
}
