using System;
using System.Globalization;
using System.Windows.Data;

namespace Noise.UI.ValueConverters {
	public class DateOnlyConverter : IValueConverter {
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			var retValue = "";

			if( value is DateTime ) {
				var date = (DateTime)value;

				retValue = date.ToShortDateString();
			}

			return( retValue );
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}
