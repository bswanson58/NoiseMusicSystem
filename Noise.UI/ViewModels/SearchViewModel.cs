using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	public class SearchType {
		public eSearchItemType	ItemType { get; private set; }
		public string			DisplayName { get; private set; }

		public SearchType( eSearchItemType itemType, string displayName ) {
			ItemType = itemType;
			DisplayName = displayName;
		}
	}

	public class SearchViewModel : DialogModelBase {
		private const int			cMaxSearchResults = 100;

		private IUnityContainer		mContainer;
		private IEventAggregator	mEventAggregator;
		private INoiseManager		mNoiseManager;
		private SearchType			mCurrentSearchType;
		private readonly List<SearchType>						mSearchTypes;
		private readonly ObservableCollectionEx<SearchViewNode>	mSearchResults;

		public SearchViewModel() {
			mSearchResults = new ObservableCollectionEx<SearchViewNode>();

			mCurrentSearchType = new SearchType( eSearchItemType.Everything, "Everything" );
			mSearchTypes = new List<SearchType> { mCurrentSearchType,
												  new SearchType( eSearchItemType.Artist, "Artists" ),
												  new SearchType( eSearchItemType.Album, "Albums" ),
												  new SearchType( eSearchItemType.Track, "Tracks" ),
												  new SearchType( eSearchItemType.BandMember, "Band Members" ),
												  new SearchType( eSearchItemType.Biography, "Biographies" ),
//												  new SearchType( eSearchItemType.Discography, "Discographies" ),
												  new SearchType( eSearchItemType.Lyrics, "Lyrics" ),
												  new SearchType( eSearchItemType.SimilarArtist, "Similar Artists" ),
												  new SearchType( eSearchItemType.TopAlbum, "Top Albums" ) };
		}

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;

				mEventAggregator = mContainer.Resolve<IEventAggregator>();
				mNoiseManager = mContainer.Resolve<INoiseManager>();
			}
		}

		public SearchType CurrentSearchType {
			get{ return( mCurrentSearchType ); }
			set{ 
				mCurrentSearchType = value;
				RaisePropertyChanged( () => CurrentSearchType );
			}
		}

		public IEnumerable<SearchType> SearchTypes {
			get{ return( mSearchTypes ); }
		}

		public string SearchText {
			get{ return( Get( () => SearchText )); }
			set{ Set( () => SearchText, value  ); }
		}

		public void Execute_Search() {
			if( mNoiseManager != null ) {
				mSearchResults.SuspendNotification();
				mSearchResults.Clear();
				mSearchResults.AddRange( BuildSearchList());

				mSearchResults.ResumeNotification();
			}
		}

		private IEnumerable<SearchViewNode> BuildSearchList() {
			var retValue = new List<SearchViewNode>();

			retValue.AddRange( from SearchResultItem item in mNoiseManager.SearchProvider.Search( CurrentSearchType.ItemType, SearchText, cMaxSearchResults ) 
							   select new SearchViewNode( item, OnNodeSelected, OnPlay ));
			return( retValue );
		}

		private void OnNodeSelected( SearchViewNode node ) {
			if( node.Artist != null ) {
				mEventAggregator.GetEvent<Events.ArtistFocusRequested>().Publish( node.Artist );
			}
			if( node.Album != null ) {
				mEventAggregator.GetEvent<Events.AlbumFocusRequested>().Publish( node.Album );
			}
		}

		private static void OnPlay( SearchViewNode node ) {
			if( node.Track != null ) {
				GlobalCommands.PlayTrack.Execute( node.Track );
			}
			else if( node.Album != null ) {
				GlobalCommands.PlayAlbum.Execute( node.Album );
			}
		}

		public ObservableCollectionEx<SearchViewNode> SearchResults {
			get{ return( mSearchResults ); }
		}
	}
}
