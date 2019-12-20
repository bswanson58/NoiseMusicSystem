using Noise.Infrastructure.Support;

namespace Noise.UI.Dto {
	public class UiArtistTrackNode : ViewModelBase {
		public	UiAlbum		Album { get; }
		public	UiTrack		Track { get; }
		public	int			Level { get; set; }
        public	bool		IsPlayable => Children.Count == 0;
        public	string		AlbumName => Children.Count > 0 ? " (multiple albums - expand to select)" : Album.Name;

		public	ObservableCollectionEx<UiArtistTrackNode>	Children { get; }

		public UiArtistTrackNode( UiTrack track, UiAlbum album ) {
			Track = track;
			Album = album;

			Children = new ObservableCollectionEx<UiArtistTrackNode>();
		}

        public void Execute_Play() {
			Track.Execute_Play();
		}

		public bool CanExecute_Play() {
			return IsPlayable;
		}
    }
}
