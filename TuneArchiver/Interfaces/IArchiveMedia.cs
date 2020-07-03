using System.Collections.Generic;
using TuneArchiver.Models;

namespace TuneArchiver.Interfaces {
    public interface IArchiveMedia {
        IList<ArchiveMedia>     MediaTypes { get; }
        long                    SizeOfMediaType( string mediaName );
    }
}
