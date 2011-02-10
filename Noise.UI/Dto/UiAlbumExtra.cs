using System.Windows.Media.Imaging;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Support;
using Noise.UI.Support;

namespace Noise.UI.Dto {
	public class UiAlbumExtra : ViewModelBase {
		public	DbArtwork	Artwork { get; private set; }
		public	DbTextInfo	TextInfo { get; private set; }
		public	BitmapImage	Image { get; private set; }
		public	bool		IsDirty { get; private set; }

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

		public void SetPreferredImage() {
			if( !Artwork.IsUserSelection ) {
				Artwork.IsUserSelection = true;

				IsDirty = true;
			}
		}

		public int ImageRotation {
			get{ return( Get( () => ImageRotation )); }
			set {
				Set( () => ImageRotation, value );

				IsDirty = true;
			}
		}

		public void Execute_RotateRight() {
			var rotation = ImageRotation + 90;

			ImageRotation = rotation < 360 ? rotation : rotation - 360;
		}

		public bool CanExecute_RotateRight() {
			return( IsImage );
		}

		public void Execute_RotateLeft() {
			var rotation = ImageRotation - 90;

			ImageRotation = rotation > 0 ? rotation : rotation + 360;
		}

		public bool CanExecute_RotateLeft() {
			return( IsImage );
		}
	}
}
