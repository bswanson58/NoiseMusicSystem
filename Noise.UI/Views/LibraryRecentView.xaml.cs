using System;
using Noise.UI.Support;
using Prism;

namespace Noise.UI.Views {
	/// <summary>
	/// Interaction logic for LibraryRecentView.xaml
	/// </summary>
	public partial class LibraryRecentView : IActiveAware {
		private readonly ActiveAwareHelper	mActiveAwareHelper;

		public	event	EventHandler		IsActiveChanged = delegate { };

		public LibraryRecentView() {
			mActiveAwareHelper = new ActiveAwareHelper( this, IsActiveChanged );

			InitializeComponent();
		}

		public bool IsActive {
			get => ( mActiveAwareHelper.IsActive );
            set => mActiveAwareHelper.IsActive = value;
        }
	}
}
