using System;
using Noise.UI.Support;
using Prism;

namespace Noise.UI.Views {
	/// <summary>
	/// Interaction logic for LibraryArtistView.xaml
	/// </summary>
	public partial class LibraryArtistView : IActiveAware {
		private readonly ActiveAwareHelper	mActiveAwareHelper;

		public	event	EventHandler		IsActiveChanged = delegate { };

		public LibraryArtistView() {
			mActiveAwareHelper = new ActiveAwareHelper( this, IsActiveChanged );

			InitializeComponent();
		}

		public bool IsActive {
			get => ( mActiveAwareHelper.IsActive );
            set => mActiveAwareHelper.IsActive = value;
        }
	}
}
