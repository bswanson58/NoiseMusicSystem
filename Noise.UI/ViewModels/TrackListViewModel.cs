using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;

namespace Noise.UI.ViewModels {
	public class TrackListViewModel {
		private IUnityContainer		mContainer;
		private IEventAggregator	mEventAggregator;
		private INoiseManager		mNoiseManager;
		private readonly ObservableCollectionEx<TrackViewNode>	mTracks;

		public TrackListViewModel() {
			mTracks = new ObservableCollectionEx<TrackViewNode>();
		}

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;

				mEventAggregator = mContainer.Resolve<IEventAggregator>();
				mNoiseManager = mContainer.Resolve<INoiseManager>();

				mEventAggregator.GetEvent<Events.ExplorerItemSelected>().Subscribe( OnExplorerItemSelected );
			}
		}

		public ObservableCollection<TrackViewNode> TrackList {
			get{ return( mTracks ); }
		}

		public void OnExplorerItemSelected( object item ) {
			mTracks.Clear();

			if( item is DbAlbum ) {
				mTracks.AddRange( from track in mNoiseManager.DataProvider.GetTrackList( item as DbAlbum ) select new TrackViewNode( mEventAggregator, track ));
			}
		}
	}
}
