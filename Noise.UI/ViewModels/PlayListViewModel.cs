using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;
using Observal;
using Observal.Extensions;

namespace Noise.UI.ViewModels {
	public class PlayListViewModel : ViewModelBase {
		private IUnityContainer			mContainer;
		private IEventAggregator		mEvents;
		private INoiseManager			mNoiseManager;
		private PlayListNode			mSelectedNode;
		private readonly Observer		mChangeObserver;
		private readonly ObservableCollectionEx<PlayListNode>	mTreeItems;

		public PlayListViewModel() {
			mTreeItems = new ObservableCollectionEx<PlayListNode>();

			mChangeObserver = new Observer();
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( OnNodeChanged );
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
			foreach( var item in mTreeItems ) {
				mChangeObserver.Release( item.UiEdit );
			}
			mTreeItems.Clear();
			mSelectedNode = null;

			foreach( var list in mNoiseManager.PlayListMgr.PlayLists ) {
				var trackList = from DbTrack track in mNoiseManager.PlayListMgr.GetTracks( list ) select track;
				var childNodes = new List<PlayListNode>();

				foreach( var track in trackList ) {
					var album = mNoiseManager.DataProvider.GetAlbumForTrack( track );
					if( album != null ) {
						var artist = mNoiseManager.DataProvider.GetArtistForAlbum( album );

						childNodes.Add( new PlayListNode( artist, album, track, OnNodeSelected, OnNodePlay ));
					}
				}

				var node = new PlayListNode( list, childNodes, OnNodeSelected, OnListPlay );
				mTreeItems.Add( node );
				mChangeObserver.Add( node.UiEdit );
			}

			mTreeItems.ResumeNotification();
		}

		private void OnNodeChanged( PropertyChangeNotification changeNotification ) {
			var notifier = changeNotification.Source as UserSettingsNotifier;

			if( notifier != null ) {
				if( changeNotification.PropertyName == "UiRating" ) {
					mNoiseManager.DataProvider.SetRating( notifier.TargetItem as DbPlayList, notifier.UiRating );
				}
				if( changeNotification.PropertyName == "UiIsFavorite" ) {
					mNoiseManager.DataProvider.SetFavorite( notifier.TargetItem as DbPlayList, notifier.UiIsFavorite );
				}
			}
		}

		private void OnNodeSelected( PlayListNode node ) {
			if( node.IsSelected ) {
				if( node.Artist != null ) {
					mEvents.GetEvent<Events.ArtistFocusRequested>().Publish( node.Artist );
				}
				if( node.Album != null ) {
					mEvents.GetEvent<Events.AlbumFocusRequested>().Publish( node.Album );
				}

				mSelectedNode = node;
			}
			else {
				mSelectedNode = null;
			}

			RaiseCanExecuteChangedEvent( "CanExecute_DeletePlayList" );
		}

		private void OnNodePlay( PlayListNode node ) {
			if( node.Track != null ) {
				mEvents.GetEvent<Events.TrackPlayRequested>().Publish( node.Track );
			}
		}

		private void OnListPlay( PlayListNode node ) {
			foreach( var track in node.TrackList ) {
				mEvents.GetEvent<Events.TrackPlayRequested>().Publish( track.Track );
			}
		}

		public void Execute_DeletePlayList() {
			if( mSelectedNode != null ) {
				mNoiseManager.PlayListMgr.Delete( mSelectedNode.PlayList );
			}
		}

		public bool CanExecute_DeletePlayList() {
			return( mSelectedNode != null );
		}
	}
}
