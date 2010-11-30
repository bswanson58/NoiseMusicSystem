using System;
using Microsoft.Practices.Prism;

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
			get {
				return mIsActive;
			}
			set {
				mIsActive = value;
				var vmAware = DataContext as IActiveAware;
				if( vmAware != null ) {
					vmAware.IsActive = value;
				}
			}
		}
	}
}
