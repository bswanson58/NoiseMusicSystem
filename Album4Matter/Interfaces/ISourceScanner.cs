using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Album4Matter.Dto;

namespace Album4Matter.Interfaces {
    public interface ISourceScanner {
        Task<IEnumerable<SourceItem>>   CollectFolder( string rootPath, Action<SourceItem> onItemInspect );
        Task                            AddTags( IEnumerable<SourceItem> items );
    }
}
