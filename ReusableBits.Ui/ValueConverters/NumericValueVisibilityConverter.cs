using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace ReusableBits.Ui.ValueConverters {
	public class NumericValueVisibilityConverter : IValueConverter, IMultiValueConverter {
		// The numerical value to be matched, defaults to zero.
		public	int		Value {  get; set; }

		// Set to true if you just want to hide the control
		// else set to false if you want to collapse the control
		public	bool	IsHidden { get; set; }

		// Set to true if you want visibility when the numerical value matches,
		// or set to false if you want visiblity when the values are not equal.
		public	bool	HideOnMatch { get; set; }

		public NumericValueVisibilityConverter() {
			Value = 0;
			IsHidden = true;
			HideOnMatch = true;
		}

		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			return( Convert( new[] { value }, targetType, parameter, culture ));
		}

		public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture ) {
			var retValue = Visibility.Visible;

			var val = values.OfType<int>().Aggregate( 0, ( current, o ) => current + o );

			if( val.Equals( Value )) {
				if( HideOnMatch ) {
					retValue = IsHidden ? Visibility.Hidden : Visibility.Collapsed;
				}
			}
			else {
				if(!HideOnMatch ) {
					retValue = IsHidden ? Visibility.Hidden : Visibility.Collapsed;
				}
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
