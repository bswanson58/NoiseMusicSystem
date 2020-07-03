using System.ComponentModel;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Support;

namespace Noise.UI.Dto {
	public class UiAlbumTrack : ViewModelBase {
        public	DbAlbum		Album { get; }
        public	UiTrack		Track { get; }
		public	bool		IsExpanded { get; set; }

		public UiAlbumTrack( DbAlbum album, UiTrack track ) {
			Album = album;
			Track = track;
        }

        public void Execute_Play() {
            Track.Execute_Play();
        }
    }

	public class UiArtistTrackNode : ViewModelBase {
		private bool		mIsExpanded;

        public	DbAlbum		Album { get; }
        public	UiTrack		Track { get; }
		public	string		TrackName => Track.Name;
        public	string		AlbumName => MultipleAlbums ? IsExpanded ? "Album List:" : $" (on {Children.Count} albums - expand to view list)" : Album.Name;
        public	bool		IsPlayable => Children.Count == 0;
		public	bool		MultipleAlbums => Children.Count > 0;

		public	ObservableCollectionEx<UiAlbumTrack>	Children { get; }

		public UiArtistTrackNode( DbAlbum album, UiTrack track ) {
			Album = album;
			Track = track;
			Children = new ObservableCollectionEx<UiAlbumTrack>();

			mIsExpanded = false;
		}

		public void AddAlbum( DbAlbum album, UiTrack track ) {
			if(!Children.Any()) {
				Children.Add( new UiAlbumTrack( Album, Track ));
            }

			Children.Add( new UiAlbumTrack( album, track ));

			Children.Sort( a => a.Album.Name, ListSortDirection.Ascending );
        }

        public bool IsExpanded {
            get => mIsExpanded;
            set {
				mIsExpanded = value;

				RaisePropertyChanged( () => AlbumName );
            }
        }

        public void Execute_Play() {
            // trigger the track queue animation
            RaisePropertyChanged( "AnimateQueueTrack" );

			Track.Execute_Play();
		}

		public bool CanExecute_Play() {
			return IsPlayable;
		}
    }
}
