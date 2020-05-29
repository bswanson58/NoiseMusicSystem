using System;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.Adapters {
	public class FavoriteViewNode : AutomaticPropertyBase, IPlayingItem {
		public	DbArtist		Artist {get; }
		public	DbAlbum			Album { get; }
		public	DbTrack			Track { get; }

        public  string          DisplayName { get; set; }
        public  string          SortingName { get; set; }

		public	DelegateCommand	PlayFavorite { get; }

		private readonly Action<FavoriteViewNode>	mPlayAction;

		private FavoriteViewNode() {
			PlayFavorite = new DelegateCommand( OnPlayFavorite );
        }

		public FavoriteViewNode( DbArtist artist, Action<FavoriteViewNode> playAction ) :
			this( artist, null, null, playAction ) {
		}

		public FavoriteViewNode( DbArtist artist, DbAlbum album, Action<FavoriteViewNode> playAction ) :
			this( artist, album, null, playAction ) {
		}

		public FavoriteViewNode( DbArtist artist, DbAlbum album, DbTrack track, Action<FavoriteViewNode> playAction ) :
            this () {
			Artist = artist;
			Album = album;
			Track = track;
			mPlayAction = playAction;
		}

		public string Title {
			get {
				var retValue = string.Empty;

				if( Track != null ) {
					retValue = $"{Track.Name} ({Artist.Name})";
				}
				else if( Album != null ) {
					retValue = $"{Album.Name} ({Artist.Name})";
				}
				else if( Artist != null ) {
					retValue = Artist.Name;
				}

				return( retValue );
			}
		}

        public bool IsPlaying {
            get { return( Get( () => IsPlaying )); }
            set {  Set( () => IsPlaying, value ); }
        }

		private void OnPlayFavorite() {
            // trigger the track queue animation
            RaisePropertyChanged( "AnimateQueueTrack" );

			mPlayAction( this );
		}

        public void SetPlayingStatus( PlayingItem item ) {
            if( Track != null ) {
                IsPlaying = Track.DbId.Equals( item.Track );
            }
            else if( Album != null ) {
                IsPlaying = Album.DbId.Equals( item.Album );
            }
            else if( Artist != null ) {
                IsPlaying = Artist.DbId.Equals( item.Artist );
            }
        }
    }
}
