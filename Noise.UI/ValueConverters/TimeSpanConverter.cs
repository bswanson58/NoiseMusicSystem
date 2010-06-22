using System;
using System.Globalization;
using System.Windows.Data;

namespace Noise.UI.ValueConverters {
	public class TimeSpanConverter : IValueConverter {
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			var retValue = "";

			if( value is TimeSpan ) {
				var timeSpan = (TimeSpan)value;

				retValue = timeSpan.ToString();
			}
			return( retValue );
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
			return( null );
		}
	}
}
