using System.Linq;
using Caliburn.Micro;
using Noise.TenFoot.Ui.Input;
using Noise.TenFoot.Ui.Interfaces;

namespace Noise.TenFoot.Ui.ViewModels {
	public class HomeViewModel : BaseListViewModel<IHomeScreen>, IHome {
		public	double							MenuListIndex { get; set; }

		public	string							ScreenTitle { get; private set; }
		public	string							Context { get; private set; }

		public HomeViewModel( IEventAggregator eventAggregator, ExitViewModel exitViewModel, ArtistListViewModel artistListViewModel,
							  FavoritesListViewModel favoritesListViewModel, QueueListViewModel queueListViewModel ) :
			base( eventAggregator ) {

			var screens = new [] { artistListViewModel as IHomeScreen, favoritesListViewModel, queueListViewModel, exitViewModel };
			ItemList.AddRange( from screen in screens orderby screen.ScreenOrder select screen );

			SelectedItem = ItemList.FirstOrDefault();

			ScreenTitle = "Noise";
			Context = string.Empty;
		}

		public void ProcessInput( InputEvent input ) {
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
			NavigateToScreen( SelectedItem.MenuCommand );
		}

		private void NavigateToScreen( eMainMenuCommand command ) {
			var screen = ( from s in ItemList where s.MenuCommand == command select s ).FirstOrDefault();

			if( screen != null ) {
				EventAggregator.Publish( new Events.NavigateToScreen( screen ));
			}
		}
	}
}
