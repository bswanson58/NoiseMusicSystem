using Caliburn.Micro;
using Noise.TenFoot.Ui.Controls.LoopingListBox;
using Noise.TenFoot.Ui.Dto;
using Noise.TenFoot.Ui.Interfaces;
using Noise.TenFoot.Ui.Views;
using ReusableBits.Mvvm.CaliburnSupport;

namespace Noise.TenFoot.Ui.ViewModels {
	public class HomeViewModel : Screen, IHome {
		private readonly BindableCollection<UiMenuItem>	mMenuChoices;
		private readonly IArtistList					mArtistList;
		private readonly IFavoritesList					mFavoritesList;
		private UiMenuItem								mSelectedMenuItem;

		private LoopingListBox	mListBox;

		public HomeViewModel( IArtistList artistListViewModel, IFavoritesList favoritesListViewModel ) {
			mArtistList = artistListViewModel;
			mFavoritesList = favoritesListViewModel;

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
			set{ 
				mSelectedMenuItem = value;
 
				Navigate( mSelectedMenuItem.Command );
			}
		}

		private void Navigate( eMainMenuCommand view ) {
			if( Parent is INavigate ) {
				var controller = Parent as INavigate;

				switch( view ) {
					case eMainMenuCommand.Library:
						controller.NavigateTo( mArtistList );
						break;

					case eMainMenuCommand.Favorites:
						controller.NavigateTo( mFavoritesList );
						break;
				}
			}
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
