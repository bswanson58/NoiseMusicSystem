using Album4Matter.Dto;

namespace Album4Matter.Interfaces {
    public interface IFileTypes {
        bool    ItemIsTextFile( SourceItem item );
        bool    ItemIsMusicFile( SourceItem item );
    }
}
