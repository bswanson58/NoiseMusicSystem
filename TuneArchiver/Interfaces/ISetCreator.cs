using System.Collections.Generic;
using TuneArchiver.Models;

namespace TuneArchiver.Interfaces {
    interface ISetCreator {
        IEnumerable<Album>  GetBestAlbumSet( List<Album> albumList );
    }
}
