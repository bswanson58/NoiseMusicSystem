using System;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Support;

namespace Noise.UI.Adapters {
	public class SearchViewNode : ViewModelBase {
		public DbArtist		Artist { get; private set; }
		public DbAlbum		Album { get; private set; }
		public DbTrack		Track { get; private set; }
		public string		Title { get; private set; }

		private readonly Action<SearchViewNode>	mOnPlay;
		private readonly Action<SearchViewNode>	mOnSelected;
		private bool							mIsSelected;

		public SearchViewNode( DbArtist artist, DbAlbum album, string title, Action<SearchViewNode> onselected, Action<SearchViewNode> onPlay ) {
			Artist = artist;
			Album = album;
			Title = title;

			mOnSelected = onselected;
			mOnPlay = onPlay;

			Track = null;
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

		public bool CanPlay {
			get{ return( Artist != null && Album != null ); }
		}

		public void Execute_Play() {
			if( mOnPlay != null ) {
				mOnPlay( this );
			}
		}
	}
}
