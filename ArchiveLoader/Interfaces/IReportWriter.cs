using System.Collections.Generic;
using ArchiveLoader.Dto;

namespace ArchiveLoader.Interfaces {
    interface IReportWriter {
        void    CreateReport( string volumeName, IEnumerable<CompletedProcessItem> items );
    }
}
