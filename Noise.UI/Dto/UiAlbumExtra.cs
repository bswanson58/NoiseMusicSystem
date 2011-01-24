using System.Windows.Media.Imaging;
using Noise.Infrastructure.Dto;
using Noise.UI.Support;

namespace Noise.UI.Dto {
	public class UiAlbumExtra {
		public	DbArtwork	Artwork { get; private set; }
		public	DbTextInfo	TextInfo { get; private set; }
		public	BitmapImage	Image { get; private set; }

		public UiAlbumExtra( DbArtwork artwork ) {
			Artwork = artwork;

			Image = BitmapUtils.CreateBitmap( artwork.Image );
		}

		public UiAlbumExtra( DbTextInfo textInfo ) {
			TextInfo = textInfo;

			Image = BitmapUtils.LoadBitmap( "Text Document.png" );
		}

		public bool IsImage {
			get{ return( Artwork != null ); }
		}

		public bool IsText {
			get{ return( TextInfo != null ); }
		}

		public string Text {
			get {
				var retValue = "";

				if( IsText ) {
					retValue = TextInfo.Text;
				}

				return( retValue );
			}
		}
	}
}
