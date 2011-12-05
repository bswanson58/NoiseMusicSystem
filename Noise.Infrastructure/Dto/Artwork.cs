namespace Noise.Infrastructure.Dto {
	public class Artwork : DbArtwork {
		public	byte[]			Image { get; set; }

		public Artwork( DbArtwork artwork ) :
			base( artwork ) {
		}
	}
}
