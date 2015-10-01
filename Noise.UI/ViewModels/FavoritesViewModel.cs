using System;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;
using CuttingEdge.Conditions;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Adapters;
using Noise.UI.Interfaces;
using Noise.UI.Logging;
using Noise.UI.Support;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class FavoritesViewModel : AutomaticCommandBase,
									  IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing>,
									  IHandle<Events.ArtistUserUpdate>, IHandle<Events.AlbumUserUpdate>, IHandle<Events.TrackUserUpdate> {
		private readonly IEventAggregator		mEventAggregator;
		private readonly IUiLog					mLog;
		private readonly IArtistProvider		mArtistProvider;
		private readonly IAlbumProvider			mAlbumProvider;
		private readonly ITrackProvider			mTrackProvider;
		private readonly IRandomTrackSelector	mTrackSelector;
		private readonly IPlayQueue				mPlayQueue;
		private readonly IDataExchangeManager	mDataExchangeMgr;
		private readonly IDialogService			mDialogService;
		private readonly ISelectionState		mSelectionState;
		private FavoriteViewNode				mSelectedNode;
		private readonly SortableCollection<FavoriteViewNode>	mFavoritesList;

		public FavoritesViewModel( IEventAggregator eventAggregator, IDatabaseInfo databaseInfo, IPlayQueue playQueue, IRandomTrackSelector trackSelector,
								   IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider, ISelectionState selectionState,
								   IDataExchangeManager dataExchangeManager, IDialogService dialogService, IUiLog log ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mTrackSelector = trackSelector;
			mPlayQueue = playQueue;
			mDataExchangeMgr = dataExchangeManager;
			mDialogService = dialogService;
			mSelectionState = selectionState;

			mEventAggregator.Subscribe( this );
			mSelectionState.CurrentArtistChanged.Subscribe( OnArtistChanged );
			mSelectionState.CurrentAlbumChanged.Subscribe( OnAlbumChanged );

			mFavoritesList = new SortableCollection<FavoriteViewNode>();

			if( databaseInfo.IsOpen ) {
				LoadFavorites();
			}
		}

		public IEventAggregator EventAggregator {
			get{ return( mEventAggregator ); }
		}

		public BindableCollection<FavoriteViewNode> FavoritesList {
			get{ return( mFavoritesList ); }
		}

		public FavoriteViewNode SelectedNode {
			get {  return( mSelectedNode ); }
			set {
				mSelectedNode = value;

				if( mSelectedNode != null ) {
					if( mSelectedNode.Artist != null ) {
						mEventAggregator.Publish( new Events.ArtistFocusRequested( mSelectedNode.Artist.DbId ));
					}
					if( mSelectedNode.Album != null ) {
						mEventAggregator.Publish( new Events.AlbumFocusRequested( mSelectedNode.Album ));
					}
				}
			}
		}

		public void Handle( Events.DatabaseOpened args ) {
			LoadFavorites();
		}

		public void Handle( Events.DatabaseClosing args ) {
			ClearFavorites();
		}

		public void Handle( Events.ArtistUserUpdate eventArgs ) {
			LoadFavorites();
		}

		public void Handle( Events.AlbumUserUpdate eventArgs ) {
			LoadFavorites();
		}

		public void Handle (Events.TrackUserUpdate eventArgs ) {
			LoadFavorites();
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

		private void LoadFavorites() {
			try {
				mFavoritesList.IsNotifying = false;
				ClearFavorites();

				using( var list = mArtistProvider.GetFavoriteArtists()) {
					foreach( var artist in list.List ) {
						mFavoritesList.Add( new FavoriteViewNode( artist, PlayArtist ) );
					}
				}

				using( var list = mAlbumProvider.GetFavoriteAlbums()) {
					foreach( var album in list.List ) {
						var artist = mArtistProvider.GetArtistForAlbum( album );

						mFavoritesList.Add( new FavoriteViewNode( artist, album, PlayAlbum ));
					}
				}
	
				using( var list = mTrackProvider.GetFavoriteTracks()) {
					foreach( var track in list.List ) {
						var album = mAlbumProvider.GetAlbumForTrack( track );

						if( album != null ) {
							var artist = mArtistProvider.GetArtistForAlbum( album );

							mFavoritesList.Add( new FavoriteViewNode( artist, album, track, PlayTrack ));
						}
					}
				}

				mFavoritesList.Sort( SelectSortProperty, ListSortDirection.Ascending );
				mFavoritesList.IsNotifying = true;
				mFavoritesList.Refresh();

				RaiseCanExecuteChangedEvent( "CanExecute_ExportFavorites" );
				RaiseCanExecuteChangedEvent( "CanExecute_PlayRandom" );
			}
			catch( Exception ex ) {
				mLog.LogException( "Loading Favorites", ex );
			}
		}

		private void ClearFavorites() {
			mFavoritesList.Clear();
		}

		private static string SelectSortProperty( FavoriteViewNode node ) {
			Condition.Requires( node ).IsNotNull();

			return( node.Track != null ? node.Track.Name : node.Album != null ? node.Album.Name : node.Artist != null ? node.Artist.Name : string.Empty );
		}

		private static void PlayArtist( FavoriteViewNode node ) {
		}

		private static void PlayAlbum( FavoriteViewNode node ) {
			GlobalCommands.PlayAlbum.Execute( node.Album );
		}

		private static void PlayTrack( FavoriteViewNode node ) {
			GlobalCommands.PlayTrack.Execute( node.Track );
		}

		public void Execute_PlayRandom() {
			mPlayQueue.Add( mTrackSelector.SelectTracksFromFavorites( track => !mPlayQueue.IsTrackQueued( track ), 10 ));
		}

		public bool CanExecute_PlayRandom() {
			return( mFavoritesList.Any());
		}

		public void Execute_ExportFavorites() {
			string fileName;

			if( mDialogService.SaveFileDialog( "Export Favorites", Constants.ExportFileExtension, "Export Files|*" + Constants.ExportFileExtension, out fileName ) == true ) {
				mDataExchangeMgr.ExportFavorites( fileName );
			}
		}

		public bool CanExecute_ExportFavorites() {
			return( mFavoritesList.Count > 0 );
		}

		public void Execute_ImportFavorites() {
			GlobalCommands.ImportFavorites.Execute( null );
		}
	}
}
