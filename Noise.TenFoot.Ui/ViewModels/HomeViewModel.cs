using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Noise.TenFoot.Ui.Dto;
using Noise.TenFoot.Ui.Input;
using Noise.TenFoot.Ui.Interfaces;

namespace Noise.TenFoot.Ui.ViewModels {
	public class HomeViewModel : BaseListViewModel<UiMenuItem>, IHome {
		private readonly IEventAggregator		mEventAggregator;
		private readonly List<IHomeScreen>		mHomeScreens; 

		public	double							MenuListIndex { get; set; }

		public	string							Title { get; private set; }
		public	string							Context { get; private set; }

		public HomeViewModel( IEventAggregator eventAggregator, ArtistListViewModel artistListViewModel,
							  FavoritesListViewModel favoritesListViewModel, QueueListViewModel queueListViewModel ) :
			base( eventAggregator ) {
			mEventAggregator = eventAggregator;

			var screens = new [] { artistListViewModel as IHomeScreen, favoritesListViewModel, queueListViewModel };
			mHomeScreens = new List<IHomeScreen>( from screen in screens orderby screen.ScreenOrder select screen );

			ItemList.AddRange( from screen in mHomeScreens select new UiMenuItem( screen.MenuCommand, screen.Title, null ));
			SelectedItem = ItemList.FirstOrDefault();

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
					NavigateToScreen( eMainMenuCommand.Library );
					break;

				case InputCommand.Favorites:
					NavigateToScreen( eMainMenuCommand.Favorites );
					break;

				case InputCommand.Queue:
					NavigateToScreen( eMainMenuCommand.Queue );
					break;
			}
		}

		protected override void DisplayItem() {
			NavigateToScreen( SelectedItem.Command );
		}

		private void NavigateToScreen( eMainMenuCommand command ) {
			var screen = ( from s in mHomeScreens where s.MenuCommand == command select s ).FirstOrDefault();

			if( screen != null ) {
				EventAggregator.Publish( new Events.NavigateToScreen( screen ));
			}
		}
	}
}
