using System.Collections.Generic;
using TuneArchiver.Models;

namespace TuneArchiver.Interfaces {
    interface IArchiveBuilder {
        void    ArchiveAlbums( IEnumerable<Album> albums, string archiveTitle );
    }
}
