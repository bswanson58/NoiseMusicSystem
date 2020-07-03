using System.Collections.Generic;

namespace ArchiveLoader.Dto {
    public class FileHandlerStorageList {
        public  IEnumerable<FileTypeHandler>    Handlers { get; }

        public FileHandlerStorageList() {
            Handlers = new List<FileTypeHandler>();
        }

        public FileHandlerStorageList( IEnumerable<FileTypeHandler> list ) {
            Handlers = new List<FileTypeHandler>( list );
        }
    }
}
