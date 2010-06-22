using System.Collections.ObjectModel;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;

namespace Noise.UI.ViewModels {
	class TrackListViewModel {
		private readonly IUnityContainer				mContainer;
		private readonly IEventAggregator				mEvents;
		private readonly ObservableCollection<DbTrack>	mTracks;

		public TrackListViewModel( IUnityContainer container ) {
			mContainer = container;
			mEvents = mContainer.Resolve<IEventAggregator>();
			mTracks = new ObservableCollection<DbTrack>();

			mEvents.GetEvent<Events.ExplorerItemSelected>().Subscribe( OnExplorerItemSelected );
		}

		public ObservableCollection<DbTrack> TrackList {
			get{ return( mTracks ); }
		}

		public void OnExplorerItemSelected( object item ) {
			
		}
	}
}
