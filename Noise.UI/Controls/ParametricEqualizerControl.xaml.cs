using System.Collections.ObjectModel;
using System.Windows;
using Noise.UI.Dto;

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
	}
}
