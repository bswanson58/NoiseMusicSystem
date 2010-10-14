using System;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.UI.Adapters {
	public class SearchViewNode : ViewModelBase {
		public DbArtist		Artist { get; private set; }
		public DbAlbum		Album { get; private set; }
		public DbTrack		Track { get; private set; }
		public string		Title { get; private set; }
		public bool			CanPlay { get; private set; }

		private readonly Action<SearchViewNode>	mOnPlay;
		private readonly Action<SearchViewNode>	mOnSelected;
		private bool							mIsSelected;

		public SearchViewNode( SearchResultItem searchResult, Action<SearchViewNode> onselected, Action<SearchViewNode> onPlay ) {
			Artist = searchResult.Artist;
			Album = searchResult.Album;
			Track = searchResult.Track;
			Title = searchResult.ItemDescription;

			if(( Album != null ) ||
			   ( Track != null )) {
				CanPlay = searchResult.ItemType == eSearchItemType.Album || searchResult.ItemType == eSearchItemType.Track;
			}

			mOnSelected = onselected;
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
