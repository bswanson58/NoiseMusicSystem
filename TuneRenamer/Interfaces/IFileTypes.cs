using TuneRenamer.Dto;

namespace TuneRenamer.Interfaces {
    public interface IFileTypes {
        bool    ItemIsMusicFile( string fileName );
        bool    ItemIsMusicFile( SourceItem item );

        bool    ItemIsTextFile( string fileName );
        bool    ItemIsTextFile( SourceItem item );
    }
}
