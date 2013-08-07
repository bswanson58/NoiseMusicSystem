using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace ReusableBits.Ui.ValueConverters {
	public class EmptyStringVisibilityConverter : IValueConverter, IMultiValueConverter {
		// Set to true if you just want to hide the control
		// else set to false if you want to collapse the control
		public	bool	IsHidden { get; set; }

		// Set to true if you want visibility when the string is empty
		// or set to false if you want visiblity when the string has length.
		public	bool	HideOnEmpty { get; set; }

		public EmptyStringVisibilityConverter() {
			IsHidden = true;
			HideOnEmpty = true;
		}

		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			return( Convert( new[] { value }, targetType, parameter, culture ));
		}

		public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture ) {
			var retValue = Visibility.Visible;

			var str = values.OfType<string>().Aggregate( "", ( current, o ) => current + o );

			if( String.IsNullOrWhiteSpace( str )) {
				if( HideOnEmpty ) {
					retValue = IsHidden ? Visibility.Hidden : Visibility.Collapsed;
				}
			}
			else {
				if(!HideOnEmpty ) {
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
