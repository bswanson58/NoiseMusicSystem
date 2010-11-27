using System.Windows;
using System.Windows.Media;

namespace Noise.UI.Controls {
	/// <summary>
	/// Interaction logic for SpectrumAnalyzerDisplay.xaml
	/// </summary>
	public partial class SpectrumAnalyzerDisplay {
		public SpectrumAnalyzerDisplay() {
			InitializeComponent();
		}

        public static DependencyProperty SpectrumImageProperty = DependencyProperty.Register( "SpectrumImage", typeof( ImageSource ), typeof( SpectrumAnalyzerDisplay ), new PropertyMetadata( null ));
        public ImageSource SpectrumImage {
            get { return GetValue( SpectrumImageProperty ) as ImageSource; }
            set { SetValue( SpectrumImageProperty, value ); }
        }

        public static DependencyProperty ImageHeightProperty = DependencyProperty.Register( "ImageHeight", typeof( double ), typeof( SpectrumAnalyzerDisplay ), new PropertyMetadata( null ));
        public double ImageHeight {
            get { return (double)GetValue( ImageHeightProperty ); }
            set { SetValue( ImageHeightProperty, value ); }
        }

        public static DependencyProperty ImageWidthProperty = DependencyProperty.Register( "ImageWidth", typeof( double ), typeof( SpectrumAnalyzerDisplay ), new PropertyMetadata( null ));
        public double ImageWidth {
            get { return (double)GetValue( ImageWidthProperty ); }
            set { SetValue( ImageWidthProperty, value ); }
        }
	}
}
