using System.Collections.Generic;
using System.Threading.Tasks;
using TuneArchiver.Models;

namespace TuneArchiver.Interfaces {
    interface IDirectoryScanner {
        Task<IEnumerable<Album>>    ScanStagingDirectory();
        Task<IEnumerable<string>>   ScanArchiveDirectory();
    }
}
