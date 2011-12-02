namespace Noise.Infrastructure.Dto {
	public class Artwork : DbArtwork {
		public	byte[]			Image { get; set; }

		public Artwork( long associatedItem, ContentType contentType ) :
			base( associatedItem, contentType ) {
		}
	}
}
