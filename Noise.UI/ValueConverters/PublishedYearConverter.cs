using System;
using System.Globalization;
using System.Windows.Data;
using Noise.Infrastructure;

namespace Noise.UI.ValueConverters {
	public class PublishedYearConverter : IValueConverter {
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			var retValue = "";

			if( value is Int32 ) {
				var year = (Int32)value;

				if( year == Constants.cUnknownYear ) {
					retValue = string.Empty;
				}
				else if( year == Constants.cVariousYears ) {
					retValue = "Various";
				}
				else {
					retValue = string.Format( "{0:D4}", year );
				}
			}
			return( retValue );
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}
