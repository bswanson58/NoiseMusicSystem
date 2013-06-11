using System.Windows;
using System.Windows.Media;

namespace Noise.UI.Controls {
	/// <summary>
	/// Interaction logic for HorizontalVuMeter.xaml
	/// </summary>
	public partial class HorizontalVuMeter {
		public HorizontalVuMeter() {
			InitializeComponent();
		}

		public static DependencyProperty LeftLevelProperty = DependencyProperty.Register( "LeftLevel", typeof( double ), typeof( HorizontalVuMeter ),
																							new PropertyMetadata( 0.0, OnLevelChanged ) );
		public double LeftLevel {
			get { return (double)GetValue( LeftLevelProperty ); }
			set { SetValue( LeftLevelProperty, value ); }
		}

		public static DependencyProperty RightLevelProperty = DependencyProperty.Register( "RightLevel", typeof( double ), typeof( HorizontalVuMeter ),
																							new PropertyMetadata( 0.0, OnLevelChanged ) );
		public double RightLevel {
			get { return (double)GetValue( RightLevelProperty ); }
			set { SetValue( RightLevelProperty, value ); }
		}

		private static void OnLevelChanged( DependencyObject sender, DependencyPropertyChangedEventArgs args ) {
			if( sender is HorizontalVuMeter ) {
				var meter = sender as HorizontalVuMeter;

				meter.CombinedLevel = ( meter.LeftLevel + meter.RightLevel ) / 2.0;
			}
		}

		public static DependencyProperty CombinedLevelProperty = DependencyProperty.Register( "CombinedLevel", typeof( double ), typeof( HorizontalVuMeter ), new PropertyMetadata( 0.0 ));
		public double CombinedLevel {
			get { return (double)GetValue( CombinedLevelProperty ); }
			set { SetValue( CombinedLevelProperty, value ); }
		}

		public static DependencyProperty PeakThresholdProperty = DependencyProperty.Register( "PeakThreshold", typeof( double ), typeof( HorizontalVuMeter ), new PropertyMetadata( 0.9 ));
		public double PeakThreshold {
			get { return (double)GetValue( PeakThresholdProperty ); }
			set { SetValue( PeakThresholdProperty, value ); }
		}

		public static DependencyProperty PeakColorProperty = DependencyProperty.Register( "PeakColor", typeof( Brush ), typeof( HorizontalVuMeter ), new PropertyMetadata( Brushes.Red ));
		public Brush PeakColor {
			get { return (Brush)GetValue( PeakColorProperty ); }
			set { SetValue( PeakColorProperty, value ); }
		}
	}
}

