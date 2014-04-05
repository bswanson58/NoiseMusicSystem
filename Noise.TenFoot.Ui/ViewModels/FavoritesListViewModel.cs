using System;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.TenFoot.Ui.Input;
using Noise.TenFoot.Ui.Interfaces;
using Noise.UI.Adapters;
using Noise.UI.Support;
using Noise.UI.ViewModels;
using Events = Noise.TenFoot.Ui.Input.Events;

namespace Noise.TenFoot.Ui.ViewModels {
	public class FavoritesListViewModel : FavoritesViewModel, IHomeScreen, IActivate, IDeactivate,
										  IHandle<InputEvent> {
		private FavoriteViewNode	mSelectedItem;
		private int					mSelectedItemIndex;

		public	event EventHandler<ActivationEventArgs>		Activated = delegate { };
		public	event EventHandler<DeactivationEventArgs>	AttemptingDeactivation = delegate { };
		public	event EventHandler<DeactivationEventArgs>	Deactivated = delegate { };

		public	bool				IsActive { get; private set; }
		public	string				ScreenTitle { get; private set; }
		public	string				MenuTitle { get; private set; }
		public	string				Description { get; private set; }
		public	string				Context { get; private set; }
		public	eMainMenuCommand	MenuCommand { get; private set; }
		public	int					ScreenOrder { get; private set; }

		public FavoritesListViewModel(IEventAggregator eventAggregator, IDatabaseInfo databaseInfo, IPlayQueue playQueue, IRandomTrackSelector trackSelector,
								      IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider,
								      IDataExchangeManager dataExchangeManager, IDialogService dialogService ) :
			base( eventAggregator, databaseInfo, playQueue, trackSelector, artistProvider, albumProvider, trackProvider, dataExchangeManager, dialogService ) {
			ScreenTitle = "Favorites";
			MenuTitle = "Favorites";
			Description = "display favorites songs";
			Context = string.Empty;
			mSelectedItemIndex = -1;

			MenuCommand = eMainMenuCommand.Favorites;
			ScreenOrder = 2;
		}

		public FavoriteViewNode SelectedItem {
			get{ return( mSelectedItem ); }
			set{ 
				mSelectedItem = value;
				mSelectedItemIndex = mSelectedItem != null ? FavoritesList.IndexOf( mSelectedItem ) : 0;

 				RaisePropertyChanged( () => SelectedItem );
			}
		}

		private void SetSelectedItem( int index ) {
			var itemCount = FavoritesList.Count;

			if( itemCount > 0 ) {
				if( index < 0 ) {
					index = itemCount + index;
				}

				if( index >= itemCount ) {
					index = index % itemCount;
				}

				if( index < itemCount ) {
					SelectedItem = FavoritesList[index];
				}
			}
		}

		private void NextItem() {
			SetSelectedItem( mSelectedItemIndex + 1 );
		}

		private void PreviousItem() {
			SetSelectedItem( mSelectedItemIndex - 1 );
		}

		private void EnqueueItem() {
			GlobalCommands.PlayTrack.Execute( SelectedItem.Track );
		}

		private void DequeueItem() {
			EventAggregator.Publish( new Events.DequeueTrack( SelectedItem.Track ));
		}

		public void Handle( InputEvent input ) {
			if( IsActive ) {
				switch( input.Command ) {
					case InputCommand.Up:
						PreviousItem();
						break;

					case InputCommand.Down:
						NextItem();
						break;

				case InputCommand.Select:
				case InputCommand.Enqueue:
					if( SelectedItem != null ) {
						EnqueueItem();
					}
					break;

				case InputCommand.Dequeue:
					if( SelectedItem != null ) {
						DequeueItem();
					}
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
