using System.Collections.ObjectModel;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.UI.ViewModels {
	class TrackListViewModel {
		private readonly IUnityContainer					mContainer;
		private readonly IEventAggregator					mEvents;
		private readonly INoiseManager						mNoiseManager;
		private readonly ObservableCollectionEx<DbTrack>	mTracks;

		public TrackListViewModel( IUnityContainer container ) {
			mContainer = container;
			mEvents = mContainer.Resolve<IEventAggregator>();
			mNoiseManager = mContainer.Resolve<INoiseManager>();
			mTracks = new ObservableCollectionEx<DbTrack>();

			mEvents.GetEvent<Events.ExplorerItemSelected>().Subscribe( OnExplorerItemSelected );
		}

		public ObservableCollection<DbTrack> TrackList {
			get{ return( mTracks ); }
		}

		public void OnExplorerItemSelected( object item ) {
			mTracks.Clear();

			if( item is DbArtist ) {
				mTracks.AddRange( mNoiseManager.DataProvider.GetTrackList( item as DbArtist ));
			}
			else if( item is DbAlbum ) {
				mTracks.AddRange( mNoiseManager.DataProvider.GetTrackList( item as DbAlbum ));
			}
		}
	}
}
