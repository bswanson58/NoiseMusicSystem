using System;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Noise.UI.ValueConverters {
	public class ByteImageConverter : IValueConverter {
		public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
			BitmapImage	retValue = null;

			if( targetType != typeof( ImageSource ) )
				throw new InvalidOperationException( "The target must be ImageSource or derived types" );

			if( value != null && value is byte[] ) {
				var bytes = value as byte[];
				if( bytes.GetLength( 0 ) > 0 ) {
					var stream = new MemoryStream( bytes );

					retValue = new BitmapImage();

					retValue.BeginInit();
					retValue.StreamSource = stream;
					retValue.EndInit();
				}
			}

			return( retValue );
		}

		public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}
