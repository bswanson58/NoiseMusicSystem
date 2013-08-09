using System;
using Noise.Infrastructure.Dto;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.Adapters {
	public class PlayHistoryNode : AutomaticCommandBase {
		private readonly Action<PlayHistoryNode>	mPlayTrackAction;

		public DbArtist		Artist { get; private set; }
		public DbAlbum		Album { get; private set; }
		public DbTrack		Track { get; private set; }
		public DateTime		PlayedOn { get; private set; }

		public PlayHistoryNode( DbArtist artist, DbAlbum album, DbTrack track, DateTime playedOn, Action<PlayHistoryNode> playTrackAction ) {
			Artist = artist;
			Album = album;
			Track = track;
			PlayedOn = playedOn;

			mPlayTrackAction = playTrackAction;
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

		public void Execute_PlayTrack() {
			if(( Track != null ) &&
			   ( mPlayTrackAction != null )) {
				mPlayTrackAction( this );
			}
		}
	}

}
