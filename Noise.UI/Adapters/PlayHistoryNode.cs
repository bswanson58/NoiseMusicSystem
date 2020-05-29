using System;
using Noise.Infrastructure.Dto;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.Adapters {
	public class PlayHistoryNode : PropertyChangeBase {
		private readonly Action<PlayHistoryNode>	mPlayTrackAction;

		public	DbArtist		Artist { get; }
		public	DbAlbum			Album { get; }
		public	DbTrack			Track { get; }
		public	DateTime		PlayedOn { get; }

		public	DelegateCommand	PlayTrack { get; }

		public PlayHistoryNode( DbArtist artist, DbAlbum album, DbTrack track, DateTime playedOn, Action<PlayHistoryNode> playTrackAction ) {
			Artist = artist;
			Album = album;
			Track = track;
			PlayedOn = playedOn;

			mPlayTrackAction = playTrackAction;

			PlayTrack = new DelegateCommand( OnPlayTrack );
		}

		public string AlbumName {
			get {
				var retValue = string.Empty;

				if( Artist != null ) {
					retValue = Artist.Name;
				}

				if( Album != null ) {
					if( !string.IsNullOrWhiteSpace( retValue )) {
						retValue = retValue + '/';
					}
					retValue = retValue + Album.Name;
				}

				return( retValue );
			}
		}

		private void OnPlayTrack() {
			if(( Track != null )) {
				mPlayTrackAction?.Invoke( this );
			}
		}
	}

}
