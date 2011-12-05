namespace Noise.Infrastructure.Dto {
	public class TextInfo : DbTextInfo {
		public	string			Text { get; set; }

		public TextInfo( DbTextInfo textInfo ) :
			base( textInfo ) {
		}

		public TextInfo( long associatedItem, ContentType contentType ) :
		base( associatedItem, contentType ) {
			Text = "";
		}
	}
}
