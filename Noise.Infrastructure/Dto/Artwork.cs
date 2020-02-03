namespace Noise.Infrastructure.Dto {
	public class Artwork : DbArtwork {
		public	byte[]			Image { get; set; }

        public  bool            HaveValidImage => ( Image != null ) && ( Image.Length > 10 );

		protected Artwork() {
			Image = new byte[0];
		}

		public Artwork( DbArtwork artwork ) :
			base( artwork ) {
		}
    }
}
