namespace Noise.Infrastructure.Dto {
	public class AlbumSupportInfo {
		public	Artwork[]		AlbumCovers { get; }
		public	Artwork[]		Artwork { get; }
		public	TextInfo[]		Info { get; }

		public AlbumSupportInfo( Artwork[] albumCover, Artwork[] artwork, TextInfo[] info ) {
			AlbumCovers = albumCover;
			Artwork = artwork;
			Info = info;
		}
	}
}
