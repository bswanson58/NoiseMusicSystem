using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuneArchiver.Models;

namespace TuneArchiver.Interfaces {
    interface ISetCreator {
        Task<IEnumerable<Album>>  GetBestAlbumSet( IList<Album> albumList, IProgress<SetCreatorProgress> progressReporter );
    }
}
