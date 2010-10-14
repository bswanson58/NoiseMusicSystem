using System;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Support;

namespace Noise.UI.Adapters {
	public class PlayHistoryNode : ViewModelBase {
		private readonly Action<PlayHistoryNode>	mPlayTrackAction;
		private readonly Action<PlayHistoryNode>	mSelectAction;
		private bool								mIsSelected;

		public DbArtist		Artist { get; private set; }
		public DbAlbum		Album { get; private set; }
		public DbTrack		Track { get; private set; }
		public DateTime		PlayedOn { get; private set; }

		public PlayHistoryNode( DbArtist artist, DbAlbum album, DbTrack track, DateTime playedOn, Action<PlayHistoryNode> onSelectAction, Action<PlayHistoryNode> playTrackAction ) {
			Artist = artist;
			Album = album;
			Track = track;
			PlayedOn = playedOn;

			mSelectAction = onSelectAction;
			mPlayTrackAction = playTrackAction;
		}

		public bool IsSelected {
			get{ return( mIsSelected ); }
			set {
				mIsSelected = value;
				if(( mIsSelected ) &&
				   ( mSelectAction != null )) {
					mSelectAction( this );
				}
			}
		}

		public void Execute_PlayTrack( object sender ) {
			var track = sender as DbTrack;

			if(( track != null ) &&
			   ( mPlayTrackAction != null )) {
				mPlayTrackAction( this );
			}
		}
	}

}
