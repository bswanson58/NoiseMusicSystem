using System.Diagnostics;

namespace TuneArchiver.Models {
    [DebuggerDisplay("Album: {" + nameof( DisplayName ) + "}")]
    class Album {
        public  string      ArtistName {get; }
        public  string      AlbumName {get; }
        public  string      Path { get; }
        public  long        Size { get; }

        public  string      DisplayName => $"{ArtistName}/{AlbumName}";

        public Album( string artist, string album, string path, long size ) {
            ArtistName = artist;
            AlbumName = album;
            Path = path;
            Size = size;
        }
    }
}
