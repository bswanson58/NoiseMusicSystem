namespace Noise.Infrastructure.Dto {
	public class ArtistSupportInfo  {
		public	TextInfo				Biography { get; private set; }
		public	Artwork					ArtistImage { get; private set; }

		public ArtistSupportInfo( TextInfo biography, Artwork artistImage ) {
			Biography = biography;
			ArtistImage = artistImage;
		}
	}
}
