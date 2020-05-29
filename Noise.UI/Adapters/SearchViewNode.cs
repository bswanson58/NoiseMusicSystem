using System;
using Noise.Infrastructure.Dto;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.Adapters {
	public class SearchViewNode : PropertyChangeBase {
	    private readonly Action<SearchViewNode> mOnPlay;

	    public  SearchResultItem    SearchItem { get; }
	    public  DbArtist            Artist => SearchItem.Artist;
	    public  DbAlbum             Album => SearchItem.Album;
	    public  DbTrack             Track => SearchItem.Track;
	    public  string              Title => SearchItem.ItemDescription;
        public  bool				CanPlay => Album != null || Track != null;

		public	DelegateCommand		Play { get; }

		public SearchViewNode( SearchResultItem searchResult, Action<SearchViewNode> onPlay ) {
			SearchItem = searchResult;
			mOnPlay = onPlay;

			Play = new DelegateCommand( OnPlay );
		}

	    private void OnPlay() {
            // trigger the track queue animation
            RaisePropertyChanged( "AnimateQueueTrack" );

	        mOnPlay?.Invoke( this );
	    }
	}
}
