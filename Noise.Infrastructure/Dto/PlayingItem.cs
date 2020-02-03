namespace Noise.Infrastructure.Dto {
    public class PlayingItem {
        public  long    Artist { get; }
        public  long    Album { get; }
        public  long    Track { get; }

        public PlayingItem() {
            Artist = Constants.cDatabaseNullOid;
            Album = Constants.cDatabaseNullOid;
            Track = Constants.cDatabaseNullOid;
        }

        public PlayingItem( DbArtist artist, DbAlbum album, DbTrack track ) {
            Artist = artist.DbId;
            Album = album.DbId;
            Track = track.DbId;
        }
    }
}
