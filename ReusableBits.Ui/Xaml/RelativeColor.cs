using System;
using System.Windows.Markup;
using System.Windows.Media;
using ReusableBits.Ui.Utility;

// from: https://stackoverflow.com/questions/10448960/assigning-custom-extension-as-static-resource-to-color-property

namespace ReusableBits.Ui.Xaml {
    [MarkupExtensionReturnType(typeof( Color ))]
    public class RelativeColor : MarkupExtension {
        private const double cEpsilon = 0.0001;

        public Color    BaseColor { get; set; }
        public double   Hue { get; set; }
        public double   Saturation { get; set; }
        public double   Luminosity { get; set; }

        public override object ProvideValue( IServiceProvider serviceProvider ) {
            var color = new HSLColor( BaseColor );

            color.Hue *= Math.Abs( Hue ) < cEpsilon ? 1f : Hue;
            color.Luminosity *= Math.Abs( Luminosity ) < cEpsilon ? 1f : Luminosity;
            color.Saturation *= Math.Abs( Saturation ) < cEpsilon ? 1f : Saturation;

            return color;
        }
    }
}
