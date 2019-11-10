using System.Collections.Generic;
using ArchiveLoader.Dto;

namespace ArchiveLoader.Interfaces {
    interface ICatalogWriter {
        void    CreateCatalog( string volumeName, IEnumerable<CompletedProcessItem> items );
    }
}
