namespace Noise.Infrastructure.Dto {
	public class AlbumSupportInfo {
		public	DbArtwork[]		AlbumCovers { get; private set; }
		public	DbArtwork[]		Artwork { get; private set; }

		public AlbumSupportInfo( DbArtwork[] albumCover, DbArtwork[] artwork ) {
			AlbumCovers = albumCover;
			Artwork = artwork;
		}
	}
}
