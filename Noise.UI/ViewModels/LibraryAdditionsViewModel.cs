﻿using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Adapters;
using Noise.UI.Interfaces;
using Noise.UI.Logging;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	internal class LibraryAdditionsViewModel : AutomaticCommandBase, IDisposable,
											   IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing>,
											   IHandle<Events.LibraryUpdateCompleted> {
		private readonly IEventAggregator		mEventAggregator;
		private readonly IUiLog					mLog;
		private readonly IArtistProvider		mArtistProvider;
		private readonly IAlbumProvider			mAlbumProvider;
		private readonly ITrackProvider			mTrackProvider;
		private readonly IPlayCommand			mPlayCommand;
        private readonly IPlayingItemHandler    mPlayingItemHandler;
		private readonly DateTime				mHorizonTime;
		private readonly UInt32					mHorizonCount;
		private IDisposable						mArtistSubscription;
		private IDisposable						mAlbumSubscription;
		private readonly BindableCollection<LibraryAdditionNode>	mNodeList;
		private LibraryAdditionNode									mSelectedNode;
		private TaskHandler<IEnumerable<LibraryAdditionNode>>		mTaskHandler;

        public BindableCollection<LibraryAdditionNode>				NodeList => ( mNodeList );

		public LibraryAdditionsViewModel( IEventAggregator eventAggregator, UserInterfacePreferences preferences, IDatabaseInfo databaseInfo, ISelectionState selectionState,
										  IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider, 
                                          IPlayCommand playCommand, IPlayingItemHandler playingItemHandler, IUiLog log ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
            mPlayingItemHandler = playingItemHandler;
			mPlayCommand = playCommand;

			mNodeList = new BindableCollection<LibraryAdditionNode>();

			mHorizonCount = preferences.NewAdditionsHorizonCount;
			mHorizonTime = DateTime.Now - new TimeSpan( preferences.NewAdditionsHorizonDays, 0, 0, 0 );

			if( databaseInfo.IsOpen ) {
				RetrieveWhatsNew();
			}

			mArtistSubscription = selectionState.CurrentArtistChanged.Subscribe( OnArtistChanged );
			mAlbumSubscription = selectionState.CurrentAlbumChanged.Subscribe( OnAlbumChanged );
            mPlayingItemHandler.StartHandler( mNodeList );
			mEventAggregator.Subscribe( this );
		}

		internal TaskHandler<IEnumerable<LibraryAdditionNode>>  TracksRetrievalTaskHandler {
			get {
				if( mTaskHandler == null ) {
					Execute.OnUIThread( () => mTaskHandler = new TaskHandler<IEnumerable<LibraryAdditionNode>>());
				}

				return( mTaskHandler );
			}
			set => mTaskHandler = value;
        }

		private void RetrieveWhatsNew() {
			TracksRetrievalTaskHandler.StartTask( RetrieveAdditions, UpdateList,
												  ex => mLog.LogException( "RetrieveWhatsNew", ex ));
		}

		public void Handle( Events.DatabaseOpened args ) {
			RetrieveWhatsNew();
		}

		public void Handle( Events.DatabaseClosing args ) {
			ClearList();
		}

		public void Handle( Events.LibraryUpdateCompleted eventArgs ) {
			RetrieveWhatsNew();

			if( eventArgs.Summary.TracksAdded > 0 ) {
				DisplayMarker = true;
			}
		}

		private void OnArtistChanged( DbArtist newArtist ) {
			if(( newArtist != null ) &&
			   ( mSelectedNode != null ) &&
			   ( mSelectedNode.Artist != null ) &&
			   ( mSelectedNode.Artist.DbId != newArtist.DbId )) {
				mSelectedNode = null;

				RaisePropertyChanged( () => SelectedNode );
			}
		}

		private void OnAlbumChanged( DbAlbum newAlbum ) {
			if(( newAlbum != null ) &&
			   ( mSelectedNode != null ) &&
			   ( mSelectedNode.Album != null ) &&
			   ( mSelectedNode.Album.DbId != newAlbum.DbId )) {
				mSelectedNode = null;
				
				RaisePropertyChanged( () => SelectedNode );
			}
		}

		private void ClearList() {
			mNodeList.Clear();
			mSelectedNode = null;
		}

		private void UpdateList( IEnumerable<LibraryAdditionNode> list ) {
			ClearList();
			mNodeList.AddRange( list );

            mPlayingItemHandler.UpdateList();
		}

		private IEnumerable<LibraryAdditionNode> RetrieveAdditions() {
			var	retValue = new List<LibraryAdditionNode>();
			var	trackList = new List<DbTrack>();
			var	albums = new Dictionary<long, long>();

			using( var additions = mTrackProvider.GetNewlyAddedTracks()) {
				if(additions?.List != null) {
					foreach( var track in additions.List ) {
						if(( albums.Count < mHorizonCount ) &&
						   ( track.DateAdded > mHorizonTime )) {
							trackList.Add( track );
						}
						else {
							break;
						}

						if(!albums.ContainsKey( track.Album )) {
							albums.Add( track.Album, track.Album );
                        }
					}
				}
			}

			if( trackList.Count > 0 ) {
				foreach( var track in trackList ) {
					var album = mAlbumProvider.GetAlbumForTrack( track );
					if( album != null ) {
						var artist = mArtistProvider.GetArtistForAlbum( album );

						if( artist != null ) {
							var treeNode = retValue.Find( node => node.Artist.DbId == artist.DbId && node.Album.DbId == album.DbId );

							if( treeNode == null ) {
								retValue.Add( new LibraryAdditionNode( artist, album, OnAlbumPlayRequested ));
							}
						}
					}
				}
			}

			if( retValue.Any()) {
				var maximumDate = DateTime.Now.Date.Ticks;
				// Make sure the minimum is at least 7 days old.
				var minimumDate = Math.Min( retValue.Min( node => node.Album.DateAddedTicks ), ( DateTime.Now - new TimeSpan( 7, 0, 0, 0 )).Ticks );

				foreach( var node in retValue ) {
					node.RelativeAge = (double)( node.Album.DateAddedTicks - minimumDate ) / ( maximumDate - minimumDate );
				}
			}

			return( retValue );
		}

        public LibraryAdditionNode SelectedNode {
			get => ( mSelectedNode );
            set {
				mSelectedNode = value;

				if( mSelectedNode != null ) {
					if( mSelectedNode.Artist != null ) {
						mEventAggregator.PublishOnUIThread( new Events.ArtistFocusRequested( mSelectedNode.Artist.DbId ));
					}
					if( mSelectedNode.Album != null ) {
						mEventAggregator.PublishOnUIThread( new Events.AlbumFocusRequested( mSelectedNode.Album ));
					}
				}
			}
		}

		private void OnAlbumPlayRequested( LibraryAdditionNode node ) {
			mPlayCommand.Play( node.Album );
		}

		public bool DisplayMarker {
			get{ return( Get( () => DisplayMarker )); }
			set{ Set( () => DisplayMarker, value ); }
		}

		public void Execute_ViewDisplayed( bool state ) {
			DisplayMarker = false;
		}

        public void Dispose() {
			mEventAggregator.Unsubscribe( this );

			mArtistSubscription?.Dispose();
			mArtistSubscription = null;

			mAlbumSubscription?.Dispose();
			mAlbumSubscription = null;

			mPlayingItemHandler.StopHandler();
        }
    }
}
