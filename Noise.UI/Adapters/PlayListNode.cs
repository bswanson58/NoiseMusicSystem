using System;
using System.Collections.Generic;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Support;

namespace Noise.UI.Adapters {
	public class PlayListNode : ViewModelBase {
		public	DbPlayList						PlayList { get; private set; }
		public	UserSettingsNotifier			UiEdit { get; private set; }
		public	DbArtist						Artist { get; private set; }
		public	DbAlbum							Album { get; private set; }
		public	DbTrack							Track { get; private set; }
		public	IEnumerable<PlayListNode>		TrackList { get; private set; }
		public	bool							IsExpanded { get; set; }
		private bool							mIsSelected;
		private readonly Action<PlayListNode>	mOnSelected;
		private readonly Action<PlayListNode>	mOnPlay;

		public PlayListNode( DbPlayList playList, IEnumerable<PlayListNode> trackList, Action<PlayListNode> onSelected, Action<PlayListNode> onPlay ) {
			PlayList = playList;
			TrackList = trackList;

			UiEdit = new UserSettingsNotifier( PlayList, null );

			mOnSelected = onSelected;
			mOnPlay = onPlay;
		}

		public PlayListNode( DbArtist artist, DbAlbum album, DbTrack track, 
							 Action<PlayListNode> onSelected, Action<PlayListNode> onPlay ) {
			Artist = artist;
			Album = album;
			Track = track;

			mOnSelected = onSelected;
			mOnPlay = onPlay;
		}

		public bool IsSelected {
			get{ return( mIsSelected ); }
			set {
				mIsSelected = value;

				if( mOnSelected != null ) {
					mOnSelected( this );
				}
			}
		}

		public void Execute_Play() {
			if( mOnPlay != null ) {
				mOnPlay( this );
			}
		}
	}
}
