using Caliburn.Micro;
using Noise.Infrastructure.Interfaces;
using Noise.TenFoot.Ui.Interfaces;
using Noise.UI.Support;
using Noise.UI.ViewModels;

namespace Noise.TenFoot.Ui.ViewModels {
	public class FavoritesListViewModel : FavoritesViewModel, ITitledScreen {
		public	string	Title { get; private set; }
		public	string	Context { get; private set; }

		public FavoritesListViewModel(IEventAggregator eventAggregator,
								   IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider,
								   IDataExchangeManager dataExchangeManager, IDialogService dialogService ) :
			base( eventAggregator, artistProvider, albumProvider, trackProvider, dataExchangeManager, dialogService ) {
			Title = "Favorites";
			Context = string.Empty;
		}
	}
}
