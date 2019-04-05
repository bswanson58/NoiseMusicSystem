using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using CuttingEdge.Conditions;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Adapters;
using Noise.UI.Interfaces;
using Noise.UI.Logging;
using Noise.UI.Support;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class FavoriteFilter {
		public string	FilterName { get; private set; }
		public bool		FilterArtists { get; private set; }
		public bool		FilterAlbums { get; private set; }
		public bool		FilterTracks { get; private set; }

		public FavoriteFilter( string name, bool filterArtists, bool filterAlbums, bool filterTracks ) {
			FilterName = name;
			FilterArtists = filterArtists;
			FilterAlbums = filterAlbums;
			FilterTracks = filterTracks;
		}
	}

	public class FavoritesViewModel : AutomaticCommandBase,
									  IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing>, IHandle<Events.LibraryUpdateCompleted>,
									  IHandle<Events.ArtistUserUpdate>, IHandle<Events.AlbumUserUpdate>, IHandle<Events.TrackUserUpdate> {
		private readonly IEventAggregator		mEventAggregator;
		private readonly IUiLog					mLog;
		private readonly IArtistProvider		mArtistProvider;
		private readonly IAlbumProvider			mAlbumProvider;
		private readonly ITrackProvider			mTrackProvider;
		private readonly IRandomTrackSelector	mTrackSelector;
		private readonly IPlayCommand			mPlayCommand;
		private readonly IPlayQueue				mPlayQueue;
		private readonly IDataExchangeManager	mDataExchangeMgr;
		private readonly IDialogService			mDialogService;
		private readonly ISelectionState		mSelectionState;
        private readonly IPlayingItemHandler    mPlayingItemHandler;
		private FavoriteViewNode				mSelectedNode;
		private ICollectionView					mFavoritesView;
		private readonly List<FavoriteFilter>	mFilterList; 
		private TaskHandler<IEnumerable<FavoriteViewNode>>		mTaskHandler; 
		private readonly SortableCollection<FavoriteViewNode>	mFavoritesList;

		public FavoritesViewModel( IEventAggregator eventAggregator, IDatabaseInfo databaseInfo, IPlayCommand playCommand, IPlayQueue playQueue, IRandomTrackSelector trackSelector,
								   IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider, ISelectionState selectionState,
								   IDataExchangeManager dataExchangeManager, IDialogService dialogService, IPlayingItemHandler playingItemHandler, IUiLog log ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mTrackSelector = trackSelector;
			mPlayCommand = playCommand;
			mPlayQueue = playQueue;
			mDataExchangeMgr = dataExchangeManager;
			mDialogService = dialogService;
			mSelectionState = selectionState;
            mPlayingItemHandler = playingItemHandler;

			mFavoritesList = new SortableCollection<FavoriteViewNode>();

			var defaultFilter = new FavoriteFilter( "All", true, true, true );
			mFilterList = new List<FavoriteFilter> { defaultFilter, 
													 new FavoriteFilter( "Artists", true, false, false ),
													 new FavoriteFilter( "Albums", false, true, false ),
													 new FavoriteFilter( "Tracks", false, false, true )};
			CurrentFilter = defaultFilter;

			mEventAggregator.Subscribe( this );
			mSelectionState.CurrentArtistChanged.Subscribe( OnArtistChanged );
			mSelectionState.CurrentAlbumChanged.Subscribe( OnAlbumChanged );
            mPlayingItemHandler.StartHandler( mFavoritesList );

			if( databaseInfo.IsOpen ) {
				LoadFavorites();
			}
		}

		public IEventAggregator EventAggregator => ( mEventAggregator );

	    public IList<FavoriteFilter> FilterList => ( mFilterList);

	    public FavoriteFilter CurrentFilter {
			get { return( Get( () => CurrentFilter )); }
			set {
				Set( () => CurrentFilter, value );

				FavoritesList.Refresh();
			}
		}

		public SortableCollection<FavoriteViewNode> FavoritesCollection => ( mFavoritesList );

	    public ICollectionView FavoritesList {
			get{ 
				if( mFavoritesView == null ) {
					mFavoritesView = CollectionViewSource.GetDefaultView( mFavoritesList );

					mFavoritesView.Filter += OnFavoriteFilter;
				}

				return( mFavoritesView );
			}
		}

		private bool OnFavoriteFilter( object node ) {
			var retValue = true;

			if( node is FavoriteViewNode ) {
				var favoriteNode = node as FavoriteViewNode;

				if( favoriteNode.Track != null ) {
					retValue = CurrentFilter.FilterTracks;
				}
				else if( favoriteNode.Album != null ) {
					retValue = CurrentFilter.FilterAlbums;
				}
				else if( favoriteNode.Artist != null ) {
					retValue = CurrentFilter.FilterArtists;
				}

			    if((!string.IsNullOrWhiteSpace( FilterText )) &&
                   ( favoriteNode.Title.IndexOf( FilterText, StringComparison.OrdinalIgnoreCase ) == -1 )) {
			        retValue = false;
			    }
			}

            return ( retValue );
		}

	    public string FilterText {
	        get { return (Get( () => FilterText )); }
	        set {
	            Execute.OnUIThread( () => {
	                Set( () => FilterText, value );

	                mFavoritesView?.Refresh();

	                RaisePropertyChanged( () => FilterText );
	            } );
	        }
	    }
		
        public FavoriteViewNode SelectedNode {
			get {  return( mSelectedNode ); }
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

		public void Handle( Events.DatabaseOpened args ) {
			LoadFavorites();
		}

		public void Handle( Events.DatabaseClosing args ) {
			ClearFavorites();
		}

		public void Handle( Events.LibraryUpdateCompleted args ) {
			LoadFavorites();
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

		internal TaskHandler <IEnumerable<FavoriteViewNode>> TaskHandler {
			get {
				if( mTaskHandler == null ) {
					Execute.OnUIThread( () => mTaskHandler = new TaskHandler<IEnumerable<FavoriteViewNode>>());
				}

				return( mTaskHandler );
			}

			set { mTaskHandler = value; }
		} 

		private void LoadFavorites() {
			TaskHandler.StartTask( RetrieveFavorites, SetFavorites, exception => mLog.LogException( "Loading Favorites", exception ));
		}

		private IEnumerable<FavoriteViewNode> RetrieveFavorites() {
			var retValue = new List<FavoriteViewNode>();

			try {
				using( var list = mArtistProvider.GetFavoriteArtists()) {
					retValue.AddRange( list.List.Select( artist => new FavoriteViewNode( artist, PlayArtist )));
				}

				using( var list = mAlbumProvider.GetFavoriteAlbums()) {
					retValue.AddRange( from album in list.List
									   let artist = mArtistProvider.GetArtistForAlbum( album )
									   select new FavoriteViewNode( artist, album, PlayAlbum ) );
				}
	
				using( var list = mTrackProvider.GetFavoriteTracks()) {
					retValue.AddRange( from track in list.List
									   let album = mAlbumProvider.GetAlbumForTrack( track ) 
										where album != null
											let artist = mArtistProvider.GetArtistForAlbum( album )
										select new FavoriteViewNode( artist, album, track, PlayTrack ));
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Loading Favorites", ex );
			}

			return( retValue );
		}

		private void SetFavorites( IEnumerable<FavoriteViewNode> favorites ) {
			mFavoritesList.IsNotifying = false;
			ClearFavorites();

			mFavoritesList.AddRange( favorites );
			mFavoritesList.Sort( SelectSortProperty, ListSortDirection.Ascending );
			mFavoritesList.IsNotifying = true;
			mFavoritesList.Refresh();

            mPlayingItemHandler.UpdateList();

			RaiseCanExecuteChangedEvent( "CanExecute_ExportFavorites" );
			RaiseCanExecuteChangedEvent( "CanExecute_PlayRandom" );
		}

		private void ClearFavorites() {
			mFavoritesList.Clear();
			mSelectedNode = null;

			RaisePropertyChanged( () => SelectedNode );
		}

		private static string SelectSortProperty( FavoriteViewNode node ) {
			Condition.Requires( node ).IsNotNull();

			return( node.Track != null ? node.Track.Name : node.Album != null ? node.Album.Name : node.Artist != null ? node.Artist.Name : string.Empty );
		}

		private void PlayArtist( FavoriteViewNode node ) {
			mPlayCommand.PlayRandomArtistTracks( node.Artist );
		}

		private void PlayAlbum( FavoriteViewNode node ) {
			mPlayCommand.Play( node.Album );
		}

		private void PlayTrack( FavoriteViewNode node ) {
			mPlayCommand.Play( node.Track );
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
