using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TuneArchiver.Models;

namespace TuneArchiver.Interfaces {
    interface IArchiveBuilder {
        Task    ArchiveAlbums( IEnumerable<Album> albums, string archiveTitle, IProgress<ArchiveBuilderProgress> progressReporter, CancellationTokenSource cancellation );
    }
}
