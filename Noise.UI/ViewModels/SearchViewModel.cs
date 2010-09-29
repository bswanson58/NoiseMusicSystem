using System.Linq;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	public class SearchViewModel : DialogModelBase {
		private IUnityContainer		mContainer;
		private IEventAggregator	mEventAggregator;
		private INoiseManager		mNoiseManager;
		private readonly ObservableCollectionEx<string>	mSearchResults;

		public SearchViewModel() {
			mSearchResults = new ObservableCollectionEx<string>();
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
				mSearchResults.Clear();

				var	searchHits = mNoiseManager.SearchProvider.Search( SearchText );
				mSearchResults.AddRange( from SearchResultItem item in searchHits select item.ItemDescription );

				RaisePropertyChanged( () => SearchResults );
			}
		}

		public ObservableCollectionEx<string> SearchResults {
			get{ return( mSearchResults ); }
		}
	}
}
