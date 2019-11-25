using System.IO;
using System.Linq;
using Album4Matter.Dto;
using Album4Matter.Interfaces;

namespace Album4Matter.Platform {
    public class FileTypes : IFileTypes {
        private readonly string[]   mTextFileExtensions = { ".txt", ".nfo" };
        private readonly string[]   mMusicFileExtensions = { ".mp3", ".flac" };

        public bool ItemIsTextFile( SourceItem item ) {
            var retValue = false;

            if( item is SourceFile ) {
                var extension = Path.GetExtension( item.FileName );

                retValue = mTextFileExtensions.Any( e => e.Equals( extension ));
            }

            return retValue;
        }

        public bool ItemIsMusicFile( SourceItem item ) {
            var retValue = false;

            if( item is SourceFile ) {
                var extension = Path.GetExtension( item.FileName );

                retValue = mMusicFileExtensions.Any( e => e.Equals( extension ));
            }

            return retValue;
        }
    }
}
