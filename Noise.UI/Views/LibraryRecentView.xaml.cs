using System;
using Microsoft.Practices.Prism;
using Noise.UI.Support;

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
			get { return( mActiveAwareHelper.IsActive ); }
			set { mActiveAwareHelper.IsActive = value; }
		}
	}
}
