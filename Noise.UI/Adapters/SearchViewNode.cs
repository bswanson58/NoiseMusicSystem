using System;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Support;

namespace Noise.UI.Adapters {
	public class SearchViewNode : ViewModelBase {
	    private readonly Action<SearchViewNode> mOnPlay;

	    public  SearchResultItem    SearchItem { get; }
		public  bool				CanPlay { get; }
	    public  DbArtist            Artist => (SearchItem.Artist);
	    public  DbAlbum             Album => (SearchItem.Album);
	    public  DbTrack             Track => (SearchItem.Track);
	    public  string              Title => (SearchItem.ItemDescription);

		public SearchViewNode( SearchResultItem searchResult, Action<SearchViewNode> onPlay ) {
			SearchItem = searchResult;

			CanPlay = ( Album != null ) || ( Track != null );

			mOnPlay = onPlay;
		}

	    public void Execute_Play() {
	        mOnPlay?.Invoke( this );
	    }
	}
}
