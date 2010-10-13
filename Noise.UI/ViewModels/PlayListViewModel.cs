using System.Linq;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;

namespace Noise.UI.ViewModels {
	public class PlayListViewModel {
		private IUnityContainer			mContainer;
		private IEventAggregator		mEvents;
		private INoiseManager			mNoiseManager;
		private readonly ObservableCollectionEx<PlayListNode>	mTreeItems;

		public PlayListViewModel() {
			mTreeItems = new ObservableCollectionEx<PlayListNode>();
		}

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;

				mEvents = mContainer.Resolve<IEventAggregator>();
				mNoiseManager = mContainer.Resolve<INoiseManager>();

				mEvents.GetEvent<Events.PlayListChanged>().Subscribe( OnPlayListChanged );

				LoadPlayLists();
			}
		}

		public ObservableCollectionEx<PlayListNode> PlayList {
			get{ return( mTreeItems ); }
		}

		private void OnPlayListChanged( IPlayListMgr playListMgr ) {
			LoadPlayLists();
		}

		private void LoadPlayLists() {
			mTreeItems.SuspendNotification();
			mTreeItems.Clear();

			foreach( var list in mNoiseManager.PlayListMgr.PlayLists ) {
				var trackList = from DbTrack track in mNoiseManager.PlayListMgr.GetTracks( list ) select new PlayListNode( track, null, OnNodeSelected, OnNodePlay );
				mTreeItems.Add( new PlayListNode( list, trackList, OnNodeSelected ));
			}

			mTreeItems.ResumeNotification();
		}

		private void OnNodeSelected( PlayListNode node ) {
			if( node.Track != null ) {
				var album = mNoiseManager.DataProvider.GetAlbumForTrack( node.Track );

				if( album != null ) {
					mEvents.GetEvent<Events.ArtistFocusRequested>().Publish( mNoiseManager.DataProvider.GetArtistForAlbum( album ));
					mEvents.GetEvent<Events.AlbumFocusRequested>().Publish( album );
				}
			}
		}

		private void OnNodePlay( PlayListNode node ) {
			if( node.Track != null ) {
				mEvents.GetEvent<Events.TrackPlayRequested>().Publish( node.Track );
			}
		}
	}
}
