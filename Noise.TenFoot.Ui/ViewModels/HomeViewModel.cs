using Caliburn.Micro;
using Noise.TenFoot.Ui.Dto;
using Noise.TenFoot.Ui.Input;
using Noise.TenFoot.Ui.Interfaces;

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

			switch( input.Command ) {
				case InputCommand.Home:
					EventAggregator.Publish( new Events.NavigateHome());
					break;

				case InputCommand.Library:
					EventAggregator.Publish( new Events.NavigateToScreen( mArtistList ));
					break;

				case InputCommand.Favorites:
					EventAggregator.Publish( new Events.NavigateToScreen( mFavoritesList ));
					break;

				case InputCommand.Queue:
					EventAggregator.Publish( new Events.NavigateToScreen( mPlayerQueue ));
					break;
			}
		}

		protected override void DisplayItem() {
			switch( SelectedItem.Command ) {
				case eMainMenuCommand.Library:
					EventAggregator.Publish( new Events.NavigateToScreen( mArtistList ));
					break;

				case eMainMenuCommand.Favorites:
					EventAggregator.Publish( new Events.NavigateToScreen( mFavoritesList ));
					break;

				case eMainMenuCommand.Queue:
					EventAggregator.Publish( new Events.NavigateToScreen( mPlayerQueue ));
					break;
			}
		}
	}
}
