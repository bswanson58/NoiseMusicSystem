using System;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Support;

namespace Noise.UI.Adapters {
	public class FavoriteViewNode : ViewModelBase {
		public	DbArtist		Artist {get; private set; }
		public	DbAlbum			Album { get; private set; }
		public	DbTrack			Track { get; private set; }
		private readonly Action<FavoriteViewNode>	mPlayAction;

		public FavoriteViewNode( DbArtist artist, Action<FavoriteViewNode> playAction ) :
			this( artist, null, null, playAction ) {
		}

		public FavoriteViewNode( DbArtist artist, DbAlbum album, Action<FavoriteViewNode> playAction ) :
			this( artist, album, null, playAction ) {
		}

		public FavoriteViewNode( DbArtist artist, DbAlbum album, DbTrack track, Action<FavoriteViewNode> playAction ) {
			Artist = artist;
			Album = album;
			Track = track;
			mPlayAction = playAction;
		}

		public string Title {
			get {
				var retValue = string.Empty;

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

		public void Execute_PlayFavorite( object sender ) {
			mPlayAction( this );
		}
	}
}
