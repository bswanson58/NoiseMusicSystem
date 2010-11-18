using System;
using System.Globalization;
using System.Windows.Data;

namespace Noise.UI.ValueConverters {
	public class NumericTextMultitudeConverter : IValueConverter {
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			var lValue = System.Convert.ToInt32( value );
			var formatValue = lValue;
			var suffix = "";

			if( Math.Abs( lValue ) > 999 ) {
				suffix = "K";

				formatValue = lValue / 1000;
			}
			if( Math.Abs( lValue ) > 999999 ) {
				suffix = "M";

				formatValue = lValue / 1000000;
			}

			return( String.Format( "{0}{1}", formatValue, suffix ));
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}
