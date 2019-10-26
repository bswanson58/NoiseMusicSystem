using System.Collections.Generic;
using TuneArchiver.Models;

namespace TuneArchiver.Interfaces {
    interface IDirectoryScanner {
        IEnumerable<Album>  ScanStagingDirectory();
        IEnumerable<Album>  ScanArchiveDirectory();
    }
}
