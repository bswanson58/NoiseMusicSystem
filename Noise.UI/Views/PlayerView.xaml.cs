using System;
using Prism;

namespace Noise.UI.Views {
	/// <summary>
	/// Interaction logic for PlayerView.xaml
	/// </summary>
	public partial class PlayerView : IActiveAware {
		private bool mIsActive;

		public event EventHandler IsActiveChanged = delegate { };

		public PlayerView() {
			InitializeComponent();
		}

		public bool IsActive {
			get => mIsActive;
            set {
				mIsActive = value;

                if( DataContext is IActiveAware vmAware ) {
					vmAware.IsActive = value;
				}
			}
		}
	}
}
