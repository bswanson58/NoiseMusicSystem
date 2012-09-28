using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure.Interfaces;
using Noise.TenFoot.Ui.Input;
using Noise.TenFoot.Ui.Interfaces;
using Noise.UI.Support;
using Noise.UI.ViewModels;

namespace Noise.TenFoot.Ui.ViewModels {
	public class QueueListViewModel : PlayQueueViewModel, ITitledScreen,
									  IHandle<Events.DequeueTrack>, IHandle<Events.DequeueAlbum> {
		public	string	Title { get; private set; }
		public	string	Context { get; private set; }

		public QueueListViewModel( IEventAggregator eventAggregator, ITagProvider tagProvider, IGenreProvider genreProvider,
								   IInternetStreamProvider internetStreamProvider, IPlayQueue playQueue,
								   IPlayListProvider playListProvider, IDialogService dialogService ) :
			base( eventAggregator, tagProvider, genreProvider, internetStreamProvider, playQueue, playListProvider, dialogService ) {
			Title = "Now Playing";
			Context = string.Empty;
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
	}
}
