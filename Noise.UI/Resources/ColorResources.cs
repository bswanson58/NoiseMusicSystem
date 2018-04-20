using System;
using System.Windows;
using System.Windows.Media;
using ReusableBits.Ui.Utility;

namespace Noise.UI.Resources {
    public static class ColorResources {
        public static Color SpectrumAnalyzerBaseColor => FindColor( "SpectrumAnalyzerBaseColor" );
        public static Color SpectrumAnalyzerPeakColor => FindColor( "SpectrumAnalyzerPeakColor" );
        public static Color SpectrumAnalyzerHoldColor => FindColor( "SpectrumAnalyzerHoldColor" );

        private static Color FindColor( String resourceName ) {
            Color retValue = Colors.Black;

            var resource = Application.Current?.MainWindow?.FindResource( resourceName );

            if( resource != null ) {
                if( resource is HSLColor hslColor ) {
                    retValue = (Color)hslColor;
                }
                else {
                    retValue = (Color)resource;
                }
            }

            return retValue;
        }
    }    
}
