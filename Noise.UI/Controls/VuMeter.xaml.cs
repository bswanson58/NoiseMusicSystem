using System.Windows;
using System.Windows.Media;

namespace Noise.UI.Controls {
	/// <summary>
	/// Interaction logic for VuMeter.xaml
	/// </summary>
	public partial class VuMeter {
		public VuMeter() {
			InitializeComponent();
		}

        public static DependencyProperty LeftLevelProperty = DependencyProperty.Register( "LeftLevel", typeof( double ), typeof( VuMeter ), new PropertyMetadata( 0.0 ));
        public double LeftLevel {
            get { return (double)GetValue( LeftLevelProperty ); }
            set { SetValue( LeftLevelProperty, value ); }
        }

        public static DependencyProperty RightLevelProperty = DependencyProperty.Register( "RightLevel", typeof( double ), typeof( VuMeter ), new PropertyMetadata( 0.0 ));
        public double RightLevel {
            get { return (double)GetValue( RightLevelProperty ); }
            set { SetValue( RightLevelProperty, value ); }
        }

        public static DependencyProperty PeakThresholdProperty = DependencyProperty.Register( "PeakThreshold", typeof( double ), typeof( VuMeter ), new PropertyMetadata( 0.9 ));
        public double PeakThreshold {
            get { return (double)GetValue( PeakThresholdProperty ); }
            set { SetValue( PeakThresholdProperty, value ); }
        }

        public static DependencyProperty PeakColorProperty = DependencyProperty.Register( "PeakColor", typeof( Brush ), typeof( VuMeter ), new PropertyMetadata( Brushes.Red ));
        public Brush PeakColor {
            get { return (Brush)GetValue( PeakColorProperty ); }
            set { SetValue( PeakColorProperty, value ); }
        }
	}
}
