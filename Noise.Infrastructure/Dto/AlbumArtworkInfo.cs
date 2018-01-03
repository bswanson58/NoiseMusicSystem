namespace Noise.Infrastructure.Dto {
    public class AlbumArtworkInfo {
        public DbArtwork[] AlbumCovers  { get; }
        public DbArtwork[] Artwork      { get; }

        public AlbumArtworkInfo( DbArtwork[] albumCover, DbArtwork[] artwork ) {
            AlbumCovers = albumCover;
            Artwork = artwork;
        }
    }
}
