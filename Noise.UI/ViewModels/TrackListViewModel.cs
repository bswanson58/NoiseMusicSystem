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
		private readonly IUnityContainer					mContainer;
		private readonly IEventAggregator					mEvents;
		private readonly INoiseManager						mNoiseManager;
		private readonly ObservableCollectionEx<TrackViewNode>	mTracks;

		public TrackListViewModel( IUnityContainer container ) {
			mContainer = container;
			mEvents = mContainer.Resolve<IEventAggregator>();
			mNoiseManager = mContainer.Resolve<INoiseManager>();
			mTracks = new ObservableCollectionEx<TrackViewNode>();

			mEvents.GetEvent<Events.ExplorerItemSelected>().Subscribe( OnExplorerItemSelected );
		}

		public ObservableCollection<TrackViewNode> TrackList {
			get{ return( mTracks ); }
		}

		public void OnExplorerItemSelected( object item ) {
			mTracks.Clear();

			if( item is DbArtist ) {
				mTracks.AddRange( from track in mNoiseManager.DataProvider.GetTrackList( item as DbArtist ) select new TrackViewNode( mEvents, track ));
			}
			else if( item is DbAlbum ) {
				mTracks.AddRange( from track in mNoiseManager.DataProvider.GetTrackList( item as DbAlbum ) select new TrackViewNode( mEvents, track ));
			}
		}
	}
}
