namespace Noise.Infrastructure.Dto {
	public class TextInfo : DbTextInfo {
		public	string			Text { get; set; }

		public TextInfo( long associatedItem, ContentType contentType ) :
		base( associatedItem, contentType ) {
			Text = "";
		}
	}
}
