namespace Noise.Infrastructure.Dto {
	public class AlbumSupportInfo {
		public	Artwork[]		AlbumCovers { get; private set; }
		public	Artwork[]		Artwork { get; private set; }
		public	TextInfo[]		Info { get; private set; }

		public AlbumSupportInfo( Artwork[] albumCover, Artwork[] artwork, TextInfo[] info ) {
			AlbumCovers = albumCover;
			Artwork = artwork;
			Info = info;
		}
	}
}
