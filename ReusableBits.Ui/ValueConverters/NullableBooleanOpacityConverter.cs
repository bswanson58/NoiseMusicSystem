using System;
using System.Globalization;
using System.Windows.Data;

namespace ReusableBits.Ui.ValueConverters {
	public class NullableBooleanOpacityConverter : IValueConverter {
		// Set to true to convert to DefaultOpacity
		// Set to false to convert to  
		public double	TrueOpacity { get; set; }
		public double	FalseOpacity { get; set; }
		public double	NullOpacity { get; set; }

		public NullableBooleanOpacityConverter() {
			TrueOpacity = 1.0D;
			NullOpacity = 0.0D;
			FalseOpacity = 0.0D;
		}

		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			var retValue = NullOpacity;

			if( value is bool? ) {
				var boolValue = value as bool?;

				if( boolValue == true ) {
					retValue = TrueOpacity;
				}
				else {
					if( boolValue == false ) {
						retValue = FalseOpacity;
					}
				}
			}

			return( retValue );
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}
