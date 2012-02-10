using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace Noise.UI.Support {
	public class BitmapUtils {
		public static BitmapImage CreateBitmap( byte[] bytes ) {
			var bitmap = new BitmapImage();

			if(( bytes != null ) &&
			   ( bytes.GetLength( 0 ) > 0 )) {
				var stream = new MemoryStream( bytes );

				bitmap.BeginInit();
				bitmap.StreamSource = stream;
				bitmap.EndInit();
			}

			return( bitmap );
		}

		public static BitmapImage LoadBitmap( string resourceName ) {
			var path = string.Format( "pack://application:,,,/Noise.UI;component/Resources/{0}", resourceName );

			return( new BitmapImage( new Uri( path )));
		}
	}
}
