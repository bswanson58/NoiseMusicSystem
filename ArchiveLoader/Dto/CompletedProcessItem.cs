using System;
using System.Linq;

namespace ArchiveLoader.Dto {
    public class CompletedProcessItem {
        public  string      Name { get; }
        public  string      Artist { get; }
        public  string      Album { get; }
        public  string      Subdirectory { get; }
        public  string      FileName { get; }
        public  string      VolumeName { get; }
        public  string      ProcessNames { get; }

        public CompletedProcessItem( ProcessItem fromItem ) {
            Name = fromItem.Name;
            FileName = fromItem.FileName;
            VolumeName = fromItem.VolumeName;
            Artist = fromItem.ArtistFolder;
            Album = fromItem.AlbumFolder;
            Subdirectory = fromItem.Subdirectory;

            if( fromItem.ProcessList.Any()) {
                ProcessNames = fromItem.ProcessList.Count > 1 ? 
                                    String.Join( ", ", from handler in fromItem.ProcessList select handler.Handler.HandlerName ) : 
                                    fromItem.ProcessList[0].Handler.HandlerName;
            }
        }
    }
}
