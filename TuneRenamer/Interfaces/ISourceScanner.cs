using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuneRenamer.Dto;

namespace TuneRenamer.Interfaces {
    public interface ISourceScanner {
        Task<IEnumerable<SourceItem>>   CollectFolder( string rootPath, Action<SourceItem> onItemInspect );
        Task                            AddTags( IEnumerable<SourceItem> items );
    }
}
