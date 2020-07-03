using System.Collections.Generic;
using System.Threading.Tasks;
using TuneRenamer.Dto;

namespace TuneRenamer.Interfaces {
    public interface IFileRenamer {
        Task<bool>  RenameFiles( IEnumerable<SourceFile> fileList );
    }
}
