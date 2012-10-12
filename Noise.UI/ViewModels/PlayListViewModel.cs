using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;
using Noise.UI.Dto;
using Observal.Extensions;

namespace Noise.UI.ViewModels {
	public class PlayListViewModel : ViewModelBase,
									 IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing>, IHandle<Events.PlayListChanged> {
		private readonly IEventAggregator	mEventAggregator;
		private readonly IArtistProvider	mArtistProvider;
		private readonly IAlbumProvider		mAlbumProvider;
		private readonly ITrackProvider		mTrackProvider;
		private readonly IPlayListProvider	mPlayListProvider;
		private PlayListNode				mSelectedNode;
		private readonly BackgroundWorker	mBackgroundWorker;
		private readonly Observal.Observer	mChangeObserver;
		private readonly ObservableCollectionEx<PlayListNode>	mTreeItems;

		public PlayListViewModel( IEventAggregator eventAggregator, IDatabaseInfo databaseInfo,
								  IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider, IPlayListProvider playListProvider ) {
			mEventAggregator = eventAggregator;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mPlayListProvider = playListProvider;

			mTreeItems = new ObservableCollectionEx<PlayListNode>();

			mBackgroundWorker = new BackgroundWorker();
			mBackgroundWorker.DoWork += ( o, args ) => args.Result = BuildPlayList();
			mBackgroundWorker.RunWorkerCompleted += ( o, result ) => UpdatePlayList( result.Result as IEnumerable<PlayListNode>);

			mChangeObserver = new Observal.Observer();
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( OnNodeChanged );

			mEventAggregator.Subscribe( this );

			if( databaseInfo.IsOpen ) {
				BackgroundLoadPlayLists();
			}
		}

		public ObservableCollectionEx<PlayListNode> PlayList {
			get{ return( mTreeItems ); }
		}

		public void Handle( Events.DatabaseOpened args ) {
			BackgroundLoadPlayLists();
		}

		public void Handle( Events.DatabaseClosing args ) {
			ClearPlayList();
		}

		public void Handle( Events.PlayListChanged eventArgs ) {
			BackgroundLoadPlayLists();
		}

		private void BackgroundLoadPlayLists() {
			if(!mBackgroundWorker.IsBusy ) {
				mBackgroundWorker.RunWorkerAsync();
			}
		}

		private IEnumerable<PlayListNode> BuildPlayList() {
			var retValue = new List<PlayListNode>();

			using( var playLists = mPlayListProvider.GetPlayLists()) {
				foreach( var list in playLists.List ) {
					var trackList = from DbTrack track in mTrackProvider.GetTrackListForPlayList( list ) select track;
					var childNodes = new List<PlayListNode>();

					foreach( var track in trackList ) {
						var album = mAlbumProvider.GetAlbumForTrack( track );
						if( album != null ) {
							var artist = mArtistProvider.GetArtistForAlbum( album );

							childNodes.Add( new PlayListNode( artist, album, track, OnNodeSelected, OnNodePlay ));
						}
					}

					retValue.Add( new PlayListNode( list, childNodes, OnNodeSelected, OnListPlay ));
				}
			}

			return( retValue );
		}

		private void ClearPlayList() {
			mTreeItems.SuspendNotification();
			foreach( var item in mTreeItems ) {
				mChangeObserver.Release( item.UiEdit );
			}
			mTreeItems.Clear();
			mSelectedNode = null;
		}

		private void UpdatePlayList( IEnumerable<PlayListNode> newList ) {
			ClearPlayList();

			mTreeItems.AddRange( newList );
			foreach( var item in mTreeItems ) {
				mChangeObserver.Add( item.UiEdit );
			}

			mTreeItems.ResumeNotification();
		}

		private static void OnNodeChanged( PropertyChangeNotification propertyNotification ) {
			var notifier = propertyNotification.Source as UiBase;

			if( notifier != null ) {
				if( propertyNotification.PropertyName == "UiRating" ) {
					GlobalCommands.SetRating.Execute( new SetRatingCommandArgs( notifier.DbId, notifier.UiRating ));
				}
				if( propertyNotification.PropertyName == "UiIsFavorite" ) {
					GlobalCommands.SetFavorite.Execute( new SetFavoriteCommandArgs( notifier.DbId, notifier.UiIsFavorite ));
				}
			}
		}

		private void OnNodeSelected( PlayListNode node ) {
			if( node.IsSelected ) {
				if( node.Artist != null ) {
					mEventAggregator.Publish( new Events.ArtistFocusRequested( node.Artist.DbId ));
				}
				if( node.Album != null ) {
					mEventAggregator.Publish( new Events.AlbumFocusRequested( node.Album ));
				}

				mSelectedNode = node;
			}
			else {
				mSelectedNode = null;
			}

			RaiseCanExecuteChangedEvent( "CanExecute_DeletePlayList" );
		}

		private static void OnNodePlay( PlayListNode node ) {
			if( node.Track != null ) {
				GlobalCommands.PlayTrack.Execute( node.Track );
			}
		}

		private static void OnListPlay( PlayListNode node ) {
			GlobalCommands.PlayTrackList.Execute( from PlayListNode children in node.TrackList select children.Track );
		}

		public void Execute_DeletePlayList() {
			if( mSelectedNode != null ) {
				mPlayListProvider.DeletePlayList( mSelectedNode.PlayList );
			}
		}

		public bool CanExecute_DeletePlayList() {
			return( mSelectedNode != null );
		}
	}
}
