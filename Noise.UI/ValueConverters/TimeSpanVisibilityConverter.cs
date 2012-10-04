using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Noise.UI.ValueConverters {
	public class TimeSpanVisibilityConverter : IValueConverter {
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			var retValue = Visibility.Hidden;

			if( value is TimeSpan ) {
				var timeSpan = (TimeSpan)value;

				if( Math.Abs( timeSpan.Ticks ) > 0 ) {
					retValue = Visibility.Visible;
				}
			}
			return( retValue );
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
			return( null );
		}
	}
}
