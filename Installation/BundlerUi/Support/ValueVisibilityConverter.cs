using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BundlerUi.Support {
	public class ValueToVisibilityConverter : IValueConverter {
		// Set to the value you want to show control
		public int TriggerValue { get; set; }

		// Set to true if you just want to hide the control
		// Set to false if you want to collapse the control
		public bool IsHidden { get; set; }

		public ValueToVisibilityConverter() {
			TriggerValue = 0;
			IsHidden = true;
		}

		private object GetVisibility( object value ) {
			if(!( value is int )) { 
				return DependencyProperty.UnsetValue;
			}

			if((int)value != TriggerValue ) {
				if( IsHidden ) {
					return Visibility.Hidden;
				}

				return Visibility.Collapsed;
			}

			return Visibility.Visible;
		}

		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			return GetVisibility( value );
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}
