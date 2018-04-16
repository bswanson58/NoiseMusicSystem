using System;
using System.Windows;
using System.Windows.Media;

namespace Noise.UI.Resources {
    public static class ColorResources {
        private static readonly ResourceDictionary mResourceDictionary;
 
        static ColorResources() {
            var uri = new Uri("/Noise.UI.Style;component/Shared/ControlStyles.xaml", UriKind.Relative);

            mResourceDictionary = (ResourceDictionary)Application.LoadComponent( uri );
        }
 
        public static Color SpectrumAnalyzerBaseColor => (Color) mResourceDictionary["SpectrumAnalyzerBaseColor"];
        public static Color SpectrumAnalyzerPeakColor => (Color) mResourceDictionary["SpectrumAnalyzerPeakColor"];
        public static Color SpectrumAnalyzerHoldColor => (Color) mResourceDictionary["SpectrumAnalyzerHoldColor"];
    }    
}
