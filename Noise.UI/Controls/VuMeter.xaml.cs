using System.Windows;

namespace Noise.UI.Controls {
	/// <summary>
	/// Interaction logic for VuMeter.xaml
	/// </summary>
	public partial class VuMeter {
		public VuMeter() {
			InitializeComponent();
		}

        public static DependencyProperty LeftLevelProperty = DependencyProperty.Register( "LeftLevel", typeof( double ), typeof( VuMeter ), new PropertyMetadata( null ));
        public double LeftLevel {
            get { return (double)GetValue( LeftLevelProperty ); }
            set { SetValue( LeftLevelProperty, value ); }
        }

        public static DependencyProperty RightLevelProperty = DependencyProperty.Register( "RightLevel", typeof( double ), typeof( VuMeter ), new PropertyMetadata( null ));
        public double RightLevel {
            get { return (double)GetValue( RightLevelProperty ); }
            set { SetValue( RightLevelProperty, value ); }
        }
	}
}
