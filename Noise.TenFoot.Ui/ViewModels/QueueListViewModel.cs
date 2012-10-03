using System;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure.Interfaces;
using Noise.TenFoot.Ui.Input;
using Noise.TenFoot.Ui.Interfaces;
using Noise.UI.Support;
using Noise.UI.ViewModels;

namespace Noise.TenFoot.Ui.ViewModels {
	public class QueueListViewModel : PlayQueueViewModel, IHomeScreen, IActivate, IDeactivate,
									  IHandle<InputEvent>, IHandle<Events.DequeueTrack>, IHandle<Events.DequeueAlbum> {
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

		public QueueListViewModel( IEventAggregator eventAggregator, ITagProvider tagProvider, IGenreProvider genreProvider,
								   IInternetStreamProvider internetStreamProvider, IPlayQueue playQueue,
								   IPlayListProvider playListProvider, IDialogService dialogService ) :
			base( eventAggregator, tagProvider, genreProvider, internetStreamProvider, playQueue, playListProvider, dialogService ) {
			ScreenTitle = "Now Playing";
			MenuTitle = "Now Playing";
			Description = "List of songs being played.";
			Context = string.Empty;

			MenuCommand = eMainMenuCommand.Queue;
			ScreenOrder = 3;
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

		public void Handle( Events.DequeueTrack track ) {
			var queuedTrack = ( from queued in QueueList where queued.QueuedTrack.Track.DbId == track.Track.DbId select queued ).FirstOrDefault();

			if( queuedTrack != null ) {
				DequeueTrack( queuedTrack.QueuedTrack );
			}
		}

		public void Handle( Events.DequeueAlbum album ) {
			var queuedTracks = ( from queued in QueueList where queued.QueuedTrack.Album.DbId == album.Album.DbId select queued ).ToList();

			foreach( var track in queuedTracks ) {
				DequeueTrack( track.QueuedTrack );
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
