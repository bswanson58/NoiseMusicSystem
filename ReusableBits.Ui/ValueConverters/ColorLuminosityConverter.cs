using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ReusableBits.Ui.Models;

namespace ReusableBits.Ui.ValueConverters {
    // from: https://www.optifunc.com/blog/hsl-color-for-wpf-and-markup-extension-to-lighten-darken-colors-in-xaml

    public class ColorLuminosityConverter : IValueConverter {
        // Amount should be 0 -> 1.0 to darken, 1.0 -> 2.0 to lighten
        public  double  Amount { get; set; }

        public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
            if( value is SolidColorBrush brush ) {
                return new SolidColorBrush( new HslColor( brush.Color ).Lighten( Amount ).ToRgb());
            }

            if( value is Color color ) {
                return new HslColor( color ).Lighten( Amount ).ToRgb();

            }

            return null;
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
            throw new NotImplementedException();
        }
    }
}
