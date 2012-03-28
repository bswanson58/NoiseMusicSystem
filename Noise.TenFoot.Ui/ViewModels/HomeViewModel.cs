using Caliburn.Micro;
using Noise.TenFoot.Ui.Controls.LoopingListBox;
using Noise.TenFoot.Ui.Dto;
using Noise.TenFoot.Ui.Views;

namespace Noise.TenFoot.Ui.ViewModels {
	public class HomeViewModel : Screen {
		private readonly BindableCollection<UiMenuItem>	mMenuChoices;
		private UiMenuItem								mSelectedMenuItem;

		private LoopingListBox	mListBox;

		public HomeViewModel() {
			mMenuChoices = new BindableCollection<UiMenuItem> { new UiMenuItem( eMainMenuCommand.Library, "Library", null ),
																new UiMenuItem( eMainMenuCommand.Favorites, "Favorites", null ),
																new UiMenuItem( eMainMenuCommand.Queue, "Queue", null ),
																new UiMenuItem( eMainMenuCommand.Search, "Search", null )};
		}

		public BindableCollection<UiMenuItem> MenuList {
			get{ return( mMenuChoices ); }
		}

		public UiMenuItem SelectedMenuList {
			get{ return( mSelectedMenuItem ); }
			set{ mSelectedMenuItem = value; }
		}

		protected override void OnViewAttached( object view, object context ) {
			base.OnViewAttached( view, context );

			mListBox = (view as HomeView).MenuList;
		}

		public void ScrollUp() {
			mListBox.Offset += 1.0;
		}

		public void ScrollDown() {
			mListBox.Offset -= 1.0;
		}
	}
}
