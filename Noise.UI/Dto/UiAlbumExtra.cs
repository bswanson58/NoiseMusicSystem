using System;
using System.Windows.Media.Imaging;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Support;
using Noise.UI.Support;

namespace Noise.UI.Dto {
	public class UiAlbumExtra : ViewModelBase {
		public	Artwork		Artwork { get; private set; }
		public	TextInfo	TextInfo { get; private set; }
		public	BitmapImage	Image { get; private set; }
		public	bool		IsDirty { get; private set; }

		public UiAlbumExtra( Artwork artwork ) {
			Artwork = artwork;

			if(( Artwork != null ) &&
			   ( Artwork.Image != null )) {
				Image = BitmapUtils.CreateBitmap( Artwork.Image );
			}
		}

		public UiAlbumExtra( TextInfo textInfo, BitmapImage image ) {
			TextInfo = textInfo;

			Image = image;
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

		public string Name {
			get{ return( IsImage ? Artwork.Name : IsText ? TextInfo.Name : "" ); }
		}

		public void SetPreferredImage() {
			if(( Artwork != null ) &&
			   (!Artwork.IsUserSelection )) {
				Artwork.IsUserSelection = true;

				IsDirty = true;
			}
		}

		public int ImageRotation {
			get{ return( Artwork != null ? Artwork.Rotation : 0 ); }
			set {
				if( Artwork != null ) {
					Artwork.Rotation = (Int16)value;

					IsDirty = true;
					Set( () => ImageRotation, value );
				}
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
