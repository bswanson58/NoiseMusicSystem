using System;
using Prism;

namespace Noise.UI.Views {
	/// <summary>
	/// Interaction logic for PlayerExtendedView.xaml
	/// </summary>
	public partial class PlayerExtendedView : IActiveAware {
		private bool mIsActive;

		public event EventHandler IsActiveChanged = delegate { };

		public PlayerExtendedView() {
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
