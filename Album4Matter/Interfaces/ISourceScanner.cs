using System;
using System.Collections.Generic;
using Album4Matter.Dto;

namespace Album4Matter.Interfaces {
    public interface ISourceScanner {
        IEnumerable<SourceItem>     CollectFolder( string rootPath, Action<SourceItem> onItemInspect );
    }
}
