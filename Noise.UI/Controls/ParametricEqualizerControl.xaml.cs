using System.Collections.ObjectModel;
using System.Windows;
using Noise.UI.Dto;
using Noise.UI.ViewModels;

namespace Noise.UI.Controls {
	/// <summary>
	/// Interaction logic for ParametricEqualizerControl.xaml
	/// </summary>
	public partial class ParametricEqualizerControl {
		public ParametricEqualizerControl() {
			InitializeComponent();
		}

        public static DependencyProperty PreampVolumeProperty = DependencyProperty.Register( "PreampVolume", typeof( double ), typeof( ParametricEqualizerControl ), new PropertyMetadata( null ));
        public double PreampVolume {
            get { return (double)GetValue( PreampVolumeProperty ); }
            set { SetValue( PreampVolumeProperty, value ); }
        }

        public static DependencyProperty EqualizerBandsProperty = DependencyProperty.Register( "EqualizerBands", typeof( ObservableCollection<UiEqBand> ), typeof( ParametricEqualizerControl ), new PropertyMetadata( null ));
        public ObservableCollection<UiEqBand> EqualizerBands {
            get { return (ObservableCollection<UiEqBand>)GetValue( EqualizerBandsProperty ); }
            set { SetValue( EqualizerBandsProperty, value ); }
        }

		private void OnPreampClick( object sender, RoutedEventArgs e ) {
			if( sender is FrameworkElement ) {
				var  element = sender as FrameworkElement;

				if( element.DataContext is PlayerViewModel ) {
					var viewModel = element.DataContext as PlayerViewModel;

					viewModel.PreampVolume = 1.0f;
				}
			}
		}

		private void OnEqBandClick( object sender, RoutedEventArgs e ) {
			if( sender is FrameworkElement ) {
				var  element = sender as FrameworkElement;

				if( element.DataContext is UiEqBand ) {
					var band = element.DataContext as UiEqBand;

					band.Gain = 0.0f;
				}
			}
		}
	}
}
