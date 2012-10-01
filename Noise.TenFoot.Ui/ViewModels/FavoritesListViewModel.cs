using System;
using Caliburn.Micro;
using Noise.Infrastructure.Interfaces;
using Noise.TenFoot.Ui.Dto;
using Noise.TenFoot.Ui.Input;
using Noise.TenFoot.Ui.Interfaces;
using Noise.UI.Support;
using Noise.UI.ViewModels;

namespace Noise.TenFoot.Ui.ViewModels {
	public class FavoritesListViewModel : FavoritesViewModel, IHomeScreen, IActivate, IDeactivate,
										  IHandle<InputEvent> {
		public	event EventHandler<ActivationEventArgs>		Activated = delegate { };
		public	event EventHandler<DeactivationEventArgs>	AttemptingDeactivation = delegate { };
		public	event EventHandler<DeactivationEventArgs>	Deactivated = delegate { };

		public	bool				IsActive { get; private set; }
		public	string				Title { get; private set; }
		public	string				Context { get; private set; }
		public	eMainMenuCommand	MenuCommand { get; private set; }
		public	int					ScreenOrder { get; private set; }

		public FavoritesListViewModel(IEventAggregator eventAggregator,
								      IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider,
								      IDataExchangeManager dataExchangeManager, IDialogService dialogService ) :
			base( eventAggregator, artistProvider, albumProvider, trackProvider, dataExchangeManager, dialogService ) {
			Title = "Favorites";
			Context = string.Empty;

			MenuCommand = eMainMenuCommand.Favorites;
			ScreenOrder = 2;
		}

		public void Handle( InputEvent input ) {
			if( IsActive ) {
				switch( input.Command ) {
					case InputCommand.Home:
						EventAggregator.Publish( new Events.NavigateHome());
						break;

					case InputCommand.Back:
						EventAggregator.Publish( new Events.NavigateReturn( this, true ));
						break;
				}
			}
		}

		public void Activate() {
			IsActive = true;

			Activated( this, new ActivationEventArgs());
		}

		public void Deactivate( bool close ) {
			IsActive = false;

			Deactivated( this, new DeactivationEventArgs());
		}
	}
}
