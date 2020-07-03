using System.Collections.Generic;
using System.Linq;
using TuneArchiver.Interfaces;

namespace TuneArchiver.Models {
    public class ArchiveMedia {
        public  string  Name { get; }
        public  long    Size { get; }

        public ArchiveMedia( string name, long size ) {
            Name = name;
            Size = size;
        }
    }

    public class ArchiveWiki : IArchiveMedia {
        public  IList<ArchiveMedia>     MediaTypes { get; }

        public ArchiveWiki() {
            MediaTypes = new List<ArchiveMedia> { new ArchiveMedia( "DVD+R", 4700372992 ), 
                                                  new ArchiveMedia( "DVD-R", 4707319808 ),
                                                  new ArchiveMedia( "DVD+R DL", 8547991552 ),
                                                  new ArchiveMedia( "DVD-R DL", 8543666176 )};
        }

        public long SizeOfMediaType( string mediaName ) {
            var retValue = 0L;
            var media = MediaTypes.FirstOrDefault( m => m.Name.Equals( mediaName ));

            if( media != null ) {
                retValue = media.Size;
            }

            return retValue;
        }
    }
}
