using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuneRenamer.Dto;

namespace TuneRenamer.Interfaces {
    public interface ISourceScanner {
        Task<List<SourceItem>>  CollectFolder( string rootPath, Action<SourceFile> onItemInspect, Action<SourceFolder> copyNames, Action<SourceFolder> copyTags );
        Task                    AddTags( IEnumerable<SourceItem> items );
    }
}
