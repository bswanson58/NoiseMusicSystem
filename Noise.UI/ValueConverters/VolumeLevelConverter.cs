using System;
using System.Globalization;
using System.Windows.Data;

namespace Noise.UI.ValueConverters {
	public class VolumeLevelConverter : IValueConverter {
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			int retValue = 0;

			if( value is double ) {
				retValue = (int)Math.Round((double)value * 100 );
			}

			return( retValue );
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}
