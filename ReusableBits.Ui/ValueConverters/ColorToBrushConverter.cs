using System;
using System.Windows.Data;
using System.Windows.Media;

namespace ReusableBits.Ui.ValueConverters {
    public class ColorToBrushConverter : IValueConverter {
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
            if( null == value ) {
                return null;
            }

            if( value is Color ) {
                var color = (Color)value;

                return new SolidColorBrush( color );
            }

            Type type = value.GetType();
            throw new InvalidOperationException( "Unsupported type [" + type.Name + "]" );            
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture ) {
            throw new NotImplementedException();
        }
    }
}
