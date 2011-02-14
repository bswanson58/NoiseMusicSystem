using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Noise.UI.ValueConverters {
	public class LevelConverter : DependencyObject, IMultiValueConverter {
		public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture ) {
			var retValue = 1.0;

			if( values[0] is int ) {
				var	level = (int)values[0];
				var	indent = (double)values[1];

				retValue = indent * level;
			}

			return( retValue );
		}

		public object[] ConvertBack( object value, Type[] targetTypes, object parameter, CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}
