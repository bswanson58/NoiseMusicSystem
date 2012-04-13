namespace Noise.Infrastructure.Dto {
	public class Artwork : DbArtwork {
		public	byte[]			Image { get; set; }

		protected Artwork() {
			Image = new byte[0];
		}

		public Artwork( DbArtwork artwork ) :
			base( artwork ) {
		}

		public bool HaveValidImage {
			get {
				return(( Image != null ) &&
					   ( Image.Length > 10 ));
			}
		}
	}
}
