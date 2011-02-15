using Noise.Infrastructure.Support;

namespace Noise.UI.Dto {
	public class UiArtistTrackNode : ViewModelBase {
		public	UiAlbum		Album { get; private set; }
		public	UiTrack		Track { get; private set; }
		public	int			Level { get; set; }

		public	ObservableCollectionEx<UiArtistTrackNode>	Children { get; private set; }

		public UiArtistTrackNode( UiTrack track, UiAlbum album ) {
			Track = track;
			Album = album;

			Children = new ObservableCollectionEx<UiArtistTrackNode>();
		}

		public string AlbumName {
			get { return( Children.Count > 0 ? " (multiple albums - expand to select)" : Album.Name ); }
		}

		public void Execute_Play() {
			Track.Execute_Play();
		}

		public bool CanExecute_Play() {
			return( IsPlayable );
		}

		public bool IsPlayable {
			get{ return( Children.Count == 0 ); }
		}
	}
}
