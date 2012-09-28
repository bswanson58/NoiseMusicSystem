using Caliburn.Micro;
using Noise.TenFoot.Ui.Dto;
using Noise.TenFoot.Ui.Input;
using Noise.TenFoot.Ui.Interfaces;
using ReusableBits.Mvvm.CaliburnSupport;

namespace Noise.TenFoot.Ui.ViewModels {
	public class HomeViewModel : Screen, IHome,
								 IHandle<InputEvent> {
		private readonly BindableCollection<UiMenuItem>	mMenuChoices;
		private readonly IEventAggregator				mEventAggregator;
		private readonly IArtistList					mArtistList;
		private readonly FavoritesListViewModel			mFavoritesList;
		private readonly QueueListViewModel				mPlayerQueue;
		private UiMenuItem								mSelectedMenuItem;

		public	double									MenuListIndex { get; set; }

		public	string									Title { get; private set; }
		public	string									Context { get; private set; }

		public HomeViewModel( IEventAggregator eventAggregator, IArtistList artistListViewModel,
							  FavoritesListViewModel favoritesListViewModel, QueueListViewModel playQueueViewModel ) {
			mEventAggregator = eventAggregator;
			mArtistList = artistListViewModel;
			mFavoritesList = favoritesListViewModel;
			mPlayerQueue = playQueueViewModel;

			mMenuChoices = new BindableCollection<UiMenuItem> { new UiMenuItem( eMainMenuCommand.Library, "Library", null ),
																new UiMenuItem( eMainMenuCommand.Favorites, "Favorites", null ),
																new UiMenuItem( eMainMenuCommand.Queue, "Queue", null ),
																new UiMenuItem( eMainMenuCommand.Search, "Search", null )};
			Title = "Noise";
			Context = string.Empty;

			mEventAggregator.Subscribe( this );
		}

		public BindableCollection<UiMenuItem> MenuList {
			get{ return( mMenuChoices ); }
		}

		public UiMenuItem SelectedMenuList {
			get{ return( mSelectedMenuItem ); }
			set{ 
				mSelectedMenuItem = value;
 
				if( mSelectedMenuItem != null ) {
					Navigate( mSelectedMenuItem.Command );
				}
			}
		}

		public void Handle( InputEvent input ) {
			if( Parent is INavigate ) {
				var controller = Parent as INavigate;

				switch( input.Command ) {
					case InputCommand.Home:
						controller.NavigateHome();
						break;

					case InputCommand.Library:
						controller.NavigateTo( mArtistList );
						break;

					case InputCommand.Favorites:
						controller.NavigateTo( mFavoritesList );
						break;

					case InputCommand.Queue:
						controller.NavigateTo( mPlayerQueue );
						break;
				}
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

					case eMainMenuCommand.Queue:
						controller.NavigateTo( mPlayerQueue );
						break;
				}
			}
		}

		protected override void OnActivate() {
			base.OnActivate();

			mSelectedMenuItem = null;
			NotifyOfPropertyChange( () => SelectedMenuList );
		}

		public void ScrollUp() {
			MenuListIndex += 1.0;

			NotifyOfPropertyChange( () => MenuListIndex );
		}

		public void ScrollDown() {
			MenuListIndex -= 1.0;

			NotifyOfPropertyChange( () => MenuListIndex );
		}
	}
}
