using Caliburn.Micro;
using Noise.TenFoot.Ui.Dto;
using Noise.TenFoot.Ui.Input;
using Noise.TenFoot.Ui.Interfaces;
using ReusableBits.Mvvm.CaliburnSupport;

namespace Noise.TenFoot.Ui.ViewModels {
	public class HomeViewModel : BaseListViewModel<UiMenuItem>, IHome {
		private readonly IEventAggregator				mEventAggregator;
		private readonly IArtistList					mArtistList;
		private readonly FavoritesListViewModel			mFavoritesList;
		private readonly QueueListViewModel				mPlayerQueue;

		public	double									MenuListIndex { get; set; }

		public	string									Title { get; private set; }
		public	string									Context { get; private set; }

		public HomeViewModel( IEventAggregator eventAggregator, IArtistList artistListViewModel,
							  FavoritesListViewModel favoritesListViewModel, QueueListViewModel playQueueViewModel ) :
			base( eventAggregator ) {
			mEventAggregator = eventAggregator;
			mArtistList = artistListViewModel;
			mFavoritesList = favoritesListViewModel;
			mPlayerQueue = playQueueViewModel;

			ItemList.Add( new UiMenuItem( eMainMenuCommand.Library, "Library", null ));
			ItemList.Add( new UiMenuItem( eMainMenuCommand.Favorites, "Favorites", null ));
			ItemList.Add( new UiMenuItem( eMainMenuCommand.Queue, "Queue", null ));
			ItemList.Add( new UiMenuItem( eMainMenuCommand.Search, "Search", null ));

			SelectedItem = ItemList[0];

			Title = "Noise";
			Context = string.Empty;

			mEventAggregator.Subscribe( this );
		}

		public override void Handle( InputEvent input ) {
			base.Handle( input );

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

		protected override void DisplayItem() {
			if( Parent is INavigate ) {
				var controller = Parent as INavigate;

				switch( SelectedItem.Command ) {
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
	}
}
