using Caliburn.Micro;
using Noise.TenFoot.Ui.Controls.LoopingListBox;
using Noise.TenFoot.Ui.Views;

namespace Noise.TenFoot.Ui.ViewModels {
	public class HomeViewModel : Screen {
		private readonly BindableCollection<string>	mMenuChoices;
		private LoopingListBox	mListBox;

		public HomeViewModel() {
			mMenuChoices = new BindableCollection<string> { "Library", "Favorites", "Queue", "What's New" };
		}

		public object MenuChoices {
			get{ return( mMenuChoices ); }
		}

		protected override void OnViewAttached( object view, object context ) {
			base.OnViewAttached( view, context );

			mListBox = (view as HomeView).listBox;
		}

		public void ScrollUp() {
			mListBox.Offset += 1.0;
		}

		public void ScrollDown() {
			mListBox.Offset -= 1.0;
		}
	}
}
