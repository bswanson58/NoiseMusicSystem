namespace Noise.Infrastructure.Dto {
	public class AlbumSupportInfo {
		public	DbArtwork[]		AlbumCovers { get; private set; }
		public	DbArtwork[]		Artwork { get; private set; }
		public	DbTextInfo[]	Info { get; private set; }

		public AlbumSupportInfo( DbArtwork[] albumCover, DbArtwork[] artwork, DbTextInfo[] info ) {
			AlbumCovers = albumCover;
			Artwork = artwork;
			Info = info;
		}
	}
}
