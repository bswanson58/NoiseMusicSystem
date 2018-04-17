using System;
using System.Windows.Markup;
using System.Windows.Media;

//
// idea from: https://stackoverflow.com/questions/31686018/new-color-from-existing-color-making-a-renamed-copy-alias-in-xaml
//
// usage:
//      <SolidColorBrush x:Key="controlBorder" Color="{ext:ColorHelper BaseColor={StaticResource colorKey}}"/>
//      <SolidColorBrush x:Key="controlBorder" Color="{ext:ColorHelper BaseBrush={StaticResource brushKey}}"/>
//      <SolidColorBrush x:Key="controlBorder" Color="{ext:ColorHelper BaseColor={StaticResource colorKey}, Brightness=-25}"/>
//      <SolidColorBrush x:Key="controlBorder" Color="{ext:ColorHelper BaseBrush={StaticResource brushKey}, Brightness=-25}"/>
//
namespace ReusableBits.Ui.Xaml {
    [MarkupExtensionReturnType(typeof( Color ))]
    public class ColorHelperExtension : MarkupExtension {
        public SolidColorBrush BaseBrush { get; set; }
        public Color BaseColor { get; set; }
        public int Brightness { get; set; }
        public int Alpha { get; set; } = -1;

        public override object ProvideValue( IServiceProvider serviceProvider ) {
            var color = BaseColor;

            if ( BaseBrush != null ) {
                color = BaseBrush.Color;
            }

            return ApplyBrightness( color, Brightness, Alpha > 0 && Alpha < 256 ? Alpha : color.A );
        }

        public static Color ApplyBrightness( Color c, int b, int alpha ) {
            byte Scale( int val ) {
                int scaledVal = val + b;

                if( scaledVal < 0 ) {
                    return 0;
                }

                if( scaledVal > 255 ) {
                    return 255;
                }

                return (byte)scaledVal;
            }

            return Color.FromArgb((byte)alpha, Scale(c.R), Scale(c.G), Scale(c.B));
        }
    }
}
