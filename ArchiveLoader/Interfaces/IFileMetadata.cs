namespace ArchiveLoader.Interfaces {
    public interface IFileMetadata {
        string  GetTrackNameFromFileName( string fileName );
        int     GetTrackNumberFromFileName( string fileName );

        string  GetAlbumNameFromAlbum( string album );
        int     GetPublishedYearFromAlbum( string albumName );
    }
}
