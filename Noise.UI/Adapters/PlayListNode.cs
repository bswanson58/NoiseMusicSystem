using System;
using System.Collections.Generic;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Support;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.Adapters {
	public class PlayListNode : PropertyChangeBase {
        private readonly Action<PlayListNode>	mOnSelected;
        private readonly Action<PlayListNode>	mOnPlay;
        private bool							mIsSelected;

        public	DbPlayList						PlayList { get; }
		public	UserSettingsNotifier			UiEdit { get; }
		public	DbArtist						Artist { get; }
		public	DbAlbum							Album { get; }
		public	DbTrack							Track { get; }
		public	TimeSpan						PlayTime { get; private set; }
		public	IEnumerable<PlayListNode>		TrackList { get; }
		public	bool							IsExpanded { get; set; }
		public	DelegateCommand					Play { get; }

		private PlayListNode() {
			Play = new DelegateCommand( OnPlay );
        }

		public PlayListNode( DbPlayList playList, IEnumerable<PlayListNode> trackList, Action<PlayListNode> onSelected, Action<PlayListNode> onPlay ) :
            this () {
			PlayList = playList;
			TrackList = trackList;

			PlayTime = new TimeSpan();
			TrackList.Each( node => { if( node.Track != null ) { PlayTime += node.Track.Duration; }});

			UiEdit = new UserSettingsNotifier( PlayList, null );

			mOnSelected = onSelected;
			mOnPlay = onPlay;
		}

		public PlayListNode( DbArtist artist, DbAlbum album, DbTrack track, Action<PlayListNode> onSelected, Action<PlayListNode> onPlay ) :
            this () {
			Artist = artist;
			Album = album;
			Track = track;

			mOnSelected = onSelected;
			mOnPlay = onPlay;
		}

		public bool IsSelected {
			get => ( mIsSelected );
            set {
				mIsSelected = value;

                mOnSelected?.Invoke( this );
            }
		}

		private void OnPlay() {
            mOnPlay?.Invoke( this );
        }
	}
}
