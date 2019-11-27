using TuneRenamer.Dto;

namespace TuneRenamer.Interfaces {
    public interface IFileTypes {
        bool    ItemIsTextFile( SourceItem item );
        bool    ItemIsMusicFile( SourceItem item );
    }
}
