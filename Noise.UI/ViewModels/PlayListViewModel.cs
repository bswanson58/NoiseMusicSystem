﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Practices.Prism.Events;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;
using Noise.UI.Dto;
using Observal.Extensions;

namespace Noise.UI.ViewModels {
	public class PlayListViewModel : ViewModelBase {
		private readonly IEventAggregator	mEvents;
		private readonly IDataProvider		mDataProvider;
		private readonly IPlayListMgr		mPlayListMgr;
		private PlayListNode				mSelectedNode;
		private readonly BackgroundWorker	mBackgroundWorker;
		private readonly Observal.Observer	mChangeObserver;
		private readonly ObservableCollectionEx<PlayListNode>	mTreeItems;

		public PlayListViewModel( IEventAggregator eventAggregator, IDataProvider dataProvider, IPlayListMgr playListMgr ) {
			mEvents = eventAggregator;
			mDataProvider = dataProvider;
			mPlayListMgr = playListMgr;

			mTreeItems = new ObservableCollectionEx<PlayListNode>();

			mBackgroundWorker = new BackgroundWorker();
			mBackgroundWorker.DoWork += ( o, args ) => args.Result = BuildPlayList();
			mBackgroundWorker.RunWorkerCompleted += ( o, result ) => UpdatePlayList( result.Result as IEnumerable<PlayListNode>);

			mChangeObserver = new Observal.Observer();
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( OnNodeChanged );

			mEvents.GetEvent<Events.PlayListChanged>().Subscribe( OnPlayListChanged );

			BackgroundLoadPlayLists();
		}

		public ObservableCollectionEx<PlayListNode> PlayList {
			get{ return( mTreeItems ); }
		}

		private void OnPlayListChanged( IPlayListMgr playListMgr ) {
			BackgroundLoadPlayLists();
		}

		private void BackgroundLoadPlayLists() {
			if(!mBackgroundWorker.IsBusy ) {
				mBackgroundWorker.RunWorkerAsync();
			}
		}

		private IEnumerable<PlayListNode> BuildPlayList() {
			var retValue = new List<PlayListNode>();

			foreach( var list in mPlayListMgr.PlayLists ) {
				var trackList = from DbTrack track in mPlayListMgr.GetTracks( list ) select track;
				var childNodes = new List<PlayListNode>();

				foreach( var track in trackList ) {
					var album = mDataProvider.GetAlbumForTrack( track );
					if( album != null ) {
						var artist = mDataProvider.GetArtistForAlbum( album );

						childNodes.Add( new PlayListNode( artist, album, track, OnNodeSelected, OnNodePlay ));
					}
				}

				retValue.Add( new PlayListNode( list, childNodes, OnNodeSelected, OnListPlay ));
			}

			return( retValue );
		}

		private void UpdatePlayList( IEnumerable<PlayListNode> newList ) {
			mTreeItems.SuspendNotification();
			foreach( var item in mTreeItems ) {
				mChangeObserver.Release( item.UiEdit );
			}
			mTreeItems.Clear();
			mSelectedNode = null;

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
				mPlayListMgr.Delete( mSelectedNode.PlayList );
			}
		}

		public bool CanExecute_DeletePlayList() {
			return( mSelectedNode != null );
		}
	}
}
