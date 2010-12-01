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
	public class SearchViewModel : DialogModelBase {
		private IUnityContainer		mContainer;
		private IEventAggregator	mEventAggregator;
		private INoiseManager		mNoiseManager;
		private readonly ObservableCollectionEx<SearchViewNode>	mSearchResults;

		public SearchViewModel() {
			mSearchResults = new ObservableCollectionEx<SearchViewNode>();
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

			retValue.AddRange( from SearchResultItem item in mNoiseManager.SearchProvider.Search( SearchText ) 
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
