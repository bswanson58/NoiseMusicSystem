using System.ComponentModel;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Support;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.Dto {
	public class UiAlbumTrack : PropertyChangeBase {
        public	DbAlbum			Album { get; }
        public	UiTrack			Track { get; }
		public	bool			IsExpanded { get; set; }

		public	DelegateCommand	Play { get; }

		public UiAlbumTrack( DbAlbum album, UiTrack track ) {
			Album = album;
			Track = track;

			Play = new DelegateCommand( OnPlay );
        }

        private void OnPlay() {
            Track.Play.Execute();
        }
    }

	public class UiArtistTrackNode : PropertyChangeBase {
		private bool				mIsExpanded;

        public	DbAlbum				Album { get; }
        public	UiTrack				Track { get; }
		public	string				TrackName => Track.Name;
        public	string				AlbumName => MultipleAlbums ? IsExpanded ? "Album List:" : $" (on {Children.Count} albums - expand to view list)" : Album.Name;
        public	bool				IsPlayable => Children.Count == 0;
		public	bool				MultipleAlbums => Children.Count > 0;
		public	DelegateCommand		Play { get; }

		public	ObservableCollectionEx<UiAlbumTrack>	Children { get; }

		public UiArtistTrackNode( DbAlbum album, UiTrack track ) {
			Album = album;
			Track = track;
			Children = new ObservableCollectionEx<UiAlbumTrack>();
			Play = new DelegateCommand( OnPlay, CanPlay );

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

        private void OnPlay() {
            // trigger the track queue animation
            RaisePropertyChanged( "AnimateQueueTrack" );

			Track.Play.Execute();
		}

		private bool CanPlay() {
			return IsPlayable;
		}
    }
}
