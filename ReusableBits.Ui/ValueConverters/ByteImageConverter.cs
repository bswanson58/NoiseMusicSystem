using System;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ReusableBits.Ui.ValueConverters {
	public class ByteImageConverter : IValueConverter {
		public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
			BitmapImage	retValue = null;

//			if( targetType != typeof( ImageSource )) { 
//				throw new InvalidOperationException( "The target must be ImageSource or derived types" );
//			}

			if( value is byte[]) {
				retValue = CreateBitmap( value as byte[]);
			}

			return( retValue );
		}

		public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
			throw new NotImplementedException();
		}

		public static BitmapImage CreateBitmap( byte[] bytes ) {
			var bitmap = new BitmapImage();

			try {
				if(( bytes != null ) &&
				   ( bytes.GetLength( 0 ) > 0 )) {
					var stream = new MemoryStream( bytes );

					bitmap.BeginInit();
					bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
					bitmap.StreamSource = stream;
					bitmap.EndInit();
				}
			}
			catch( Exception ) {
				bitmap = null;
			}

			return( bitmap );
		}
	}
}
