using System;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Support;

namespace Noise.UI.Adapters {
	public class SearchViewNode : ViewModelBase {
		public SearchResultItem SearchItem { get; private set; }
		public bool				CanPlay { get; private set; }

		private readonly Action<SearchViewNode>	mOnPlay;
		private readonly Action<SearchViewNode>	mOnSelected;
		private bool							mIsSelected;

		public SearchViewNode( SearchResultItem searchResult, Action<SearchViewNode> onselected, Action<SearchViewNode> onPlay ) {
			SearchItem = searchResult;

			CanPlay = ( Album != null ) || ( Track != null );

			mOnSelected = onselected;
			mOnPlay = onPlay;
		}

		public DbArtist Artist {
			get {  return( SearchItem.Artist ); }
		}

		public DbAlbum Album {
			get {  return( SearchItem.Album ); }
		}

		public DbTrack Track {
			get {  return( SearchItem.Track ); }
		}

		public string Title {
			get{ return( SearchItem.ItemDescription ); }
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
