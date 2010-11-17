using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Noise.UI.ValueConverters {
	public class EmptyStringVisibilityConverter : IValueConverter, IMultiValueConverter {
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			return( Convert( new[] { value }, targetType, parameter, culture ));
		}

		public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture ) {
			var retValue = Visibility.Visible;

			var str = values.OfType<string>().Aggregate( "", ( current, o ) => current + o );

			if( String.IsNullOrWhiteSpace( str )) {
				retValue = Visibility.Collapsed;
			}

			return( retValue );
		}

		public object[] ConvertBack( object value, Type[] targetType, object parameter, CultureInfo culture ) {
			throw new NotImplementedException();
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}
