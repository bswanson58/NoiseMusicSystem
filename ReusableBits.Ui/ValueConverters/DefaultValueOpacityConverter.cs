using System;
using System.Globalization;
using System.Windows.Data;

namespace ReusableBits.Ui.ValueConverters {
	//
	// Converts between any object type to an opacity (double) value.
	// If the object is equal to it's default value a 'default opacity value' of 0.0D is returned.
	// For any other non-default object value a value of 1.0D is returned.
	// The values returned for opacity can be set in the ConverterParameter values separated by a '|' character:
	// Opacity="{Binding Path=SomeProperty, Converter={StaticResource DefaultValueOpacityConverter}, ConverterParameter=0.1|0.9}"
	// 
	[ValueConversion( typeof( object ), typeof( double ))]
	public class DefaultValueOpacityConverter : IValueConverter {
		public double	StandardOpacity { get; set; }
		public double	DefaultOpacity { get; set; }

		public DefaultValueOpacityConverter() {
			DefaultOpacity = 0.0D;
			StandardOpacity = 1.0D;
		}

		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			bool hasDefaultValue;

			if( value is string ) {
				hasDefaultValue = !String.IsNullOrEmpty( value.ToString());
			}
			else {
				var defaultValue = value != null ? value.GetType().GetDefaultValue() : null;

				hasDefaultValue = Equals( value, defaultValue );
			}

			return hasDefaultValue ? DefaultOpacity : StandardOpacity;
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}

	internal static class TypeExtension {
		public static object GetDefaultValue( this Type targetType ) {
			return targetType.IsValueType ? Activator.CreateInstance( targetType ) : null;
		}
	}
}
