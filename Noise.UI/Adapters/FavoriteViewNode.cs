using System;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Support;

namespace Noise.UI.Adapters {
	public class FavoriteViewNode : ViewModelBase {
		public	DbArtist		Artist {get; private set; }
		public	DbAlbum			Album { get; private set; }
		public	DbTrack			Track { get; private set; }
		private bool			mIsSelected;
		private readonly Action<FavoriteViewNode>	mPlayAction;
		private readonly Action<FavoriteViewNode>	mSelectAction;

		public FavoriteViewNode( DbArtist artist, Action<FavoriteViewNode> playAction, Action<FavoriteViewNode> selectAction ) :
			this( artist, null, null, playAction, selectAction ) {
		}

		public FavoriteViewNode( DbArtist artist, DbAlbum album, Action<FavoriteViewNode> playAction, Action<FavoriteViewNode> selectAction ) :
			this( artist, album, null, playAction, selectAction ) {
		}

		public FavoriteViewNode( DbArtist artist, DbAlbum album, DbTrack track, Action<FavoriteViewNode> playAction, Action<FavoriteViewNode> selectAction ) {
			Artist = artist;
			Album = album;
			Track = track;
			mPlayAction = playAction;
			mSelectAction = selectAction;
		}

		public string Title {
			get {
				var retValue = "";

				if( Track != null ) {
					retValue = string.Format( "{0} ({1})", Track.Name, Artist.Name );
				}
				else if( Album != null ) {
					retValue = string.Format( "{0} ({1})", Album.Name, Artist.Name );
				}
				else if( Artist != null ) {
					retValue = Artist.Name;
				}

				return( retValue );
			}
		}

		public bool IsSelected {
			get { return mIsSelected; }
			set {
				if( value != mIsSelected ) {
					mIsSelected = value;

					RaisePropertyChanged( () => IsSelected );

					mSelectAction( this );
				}
			}
		}

		public void Execute_PlayFavorite( object sender ) {
			mPlayAction( this );
		}
	}
}
