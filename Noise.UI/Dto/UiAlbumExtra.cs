using System;
using System.Windows.Media.Imaging;
using Noise.Infrastructure.Dto;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;
using ReusableBits.Ui.ValueConverters;

namespace Noise.UI.Dto {
	public class UiAlbumExtra : AutomaticPropertyBase {
		public	Artwork			Artwork { get; }
		public	TextInfo		TextInfo { get; }
		public	BitmapImage		Image { get; }
		public	bool			IsDirty { get; private set; }
        public	bool			IsImage => Artwork != null;
        public	bool			IsText => TextInfo != null;
        public	string			Name => IsImage ? Artwork.Name : IsText ? TextInfo.Name : String.Empty;

		public	DelegateCommand	RotateRight { get; }
		public	DelegateCommand	RotateLeft { get ; }

		private UiAlbumExtra() {
            RotateLeft = new DelegateCommand( OnRotateLeft, CanRotateLeft );
            RotateRight = new DelegateCommand( OnRotateRight, CanRotateRight );
        }

		public UiAlbumExtra( Artwork artwork ) : 
            this () {
			Artwork = artwork;

			if(Artwork?.Image != null) {
				Image = ByteImageConverter.CreateBitmap( Artwork.Image );
			}

		}

		public UiAlbumExtra( TextInfo textInfo, BitmapImage image ) :
            this () {
			TextInfo = textInfo;

			Image = image;
		}


        public string Text {
			get {
				var retValue = String.Empty;

				if( IsText ) {
					retValue = TextInfo.Text;
				}

				return retValue;
			}
		}


        public void SetPreferredImage() {
			if(( Artwork != null ) &&
			   (!Artwork.IsUserSelection )) {
				Artwork.IsUserSelection = true;

				IsDirty = true;
			}
		}

		public int ImageRotation {
			get => Artwork?.Rotation ?? 0;
            set {
				if( Artwork != null ) {
					Artwork.Rotation = (Int16)value;

					IsDirty = true;
					Set( () => ImageRotation, value );
				}
			}
		}

		private void OnRotateRight() {
			var rotation = ImageRotation + 90;

			ImageRotation = rotation < 360 ? rotation : rotation - 360;
		}

		private bool CanRotateRight() {
			return( IsImage );
		}

		private void OnRotateLeft() {
			var rotation = ImageRotation - 90;

			ImageRotation = rotation > 0 ? rotation : rotation + 360;
		}

		private bool CanRotateLeft() {
			return( IsImage );
		}
	}
}
