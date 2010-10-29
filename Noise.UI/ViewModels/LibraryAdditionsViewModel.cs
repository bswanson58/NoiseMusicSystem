using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;

namespace Noise.UI.ViewModels {
	public class LibraryAdditionsViewModel {
		private IUnityContainer				mContainer;
		private IEventAggregator			mEvents;
		private INoiseManager				mNoiseManager;
		private readonly DateTime			mHorizonTime;
		private readonly UInt32				mHorizonCount;
		private readonly BackgroundWorker	mBackgroundWorker;
		private readonly ObservableCollectionEx<LibraryAdditionNode>	mNodeList;

		public LibraryAdditionsViewModel() {
			mNodeList = new ObservableCollectionEx<LibraryAdditionNode>();

			mHorizonTime = DateTime.Now - new TimeSpan( 3, 0, 0, 0 );
			mHorizonCount = 100;

			mBackgroundWorker = new BackgroundWorker();
			mBackgroundWorker.DoWork += ( o, args ) => args.Result = RetrieveAdditions();
			mBackgroundWorker.RunWorkerCompleted += ( o, result ) => UpdateList( result.Result as IEnumerable<LibraryAdditionNode>);
		}

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;

				mEvents = mContainer.Resolve<IEventAggregator>();
				mNoiseManager = mContainer.Resolve<INoiseManager>();

				mBackgroundWorker.RunWorkerAsync();
			}
		}

		private void UpdateList( IEnumerable<LibraryAdditionNode> list ) {
			mNodeList.SuspendNotification();
			mNodeList.Clear();
			mNodeList.AddRange( list );
			mNodeList.ResumeNotification();
		}

		private IEnumerable<LibraryAdditionNode> RetrieveAdditions() {
			var	retValue = new List<LibraryAdditionNode>();
			var	trackList = new List<DbTrack>();

			using( var additions = mNoiseManager.DataProvider.GetNewlyAddedTracks()) {
				UInt32	count = 0;

				foreach( var track in additions.List ) {
					if(( count < mHorizonCount ) &&
					   ( track.DateAdded > mHorizonTime )) {
						trackList.Add( track );
					}
					else {
						break;
					}

					count++;
				}
			}

			if( trackList.Count > 0 ) {
				
				foreach( var track in trackList ) {
					var album = mNoiseManager.DataProvider.GetAlbumForTrack( track );
					if( album != null ) {
						var artist = mNoiseManager.DataProvider.GetArtistForAlbum( album );

						if( artist != null ) {
							var treeNode = retValue.Find( node => node.Artist.DbId == artist.DbId && node.Album.DbId == album.DbId );

							if( treeNode != null ) {
								treeNode.AddTrack( track );
							}

							else {
								retValue.Add( new LibraryAdditionNode( artist, album, track, OnNodeSelected, OnAlbumPlayRequested, OnTrackPlayRequested ));
							}
						}
					}
				}
			}

			return( retValue );
		}

		public ObservableCollectionEx<LibraryAdditionNode> NodeList {
			get{ return( mNodeList ); }
		}

		private void OnNodeSelected( LibraryAdditionNode node ) {
			if( node.Artist != null ) {
				mEvents.GetEvent<Events.ArtistFocusRequested>().Publish( node.Artist );
			}
			if( node.Album != null ) {
				mEvents.GetEvent<Events.AlbumFocusRequested>().Publish( node.Album );
			}
		}

		private void OnAlbumPlayRequested( LibraryAdditionNode node ) {
			foreach( var track in node.TrackList ) {
				mEvents.GetEvent<Events.TrackPlayRequested>().Publish( mNoiseManager.DataProvider.GetTrack( track.DbId ));
			}
		}

		private void OnTrackPlayRequested( long trackId ) {
			mEvents.GetEvent<Events.TrackPlayRequested>().Publish( mNoiseManager.DataProvider.GetTrack( trackId ));
		}
	}
}
