using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Adapters;
using Noise.UI.Interfaces;
using Noise.UI.Logging;
using Prism.Commands;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;
using ReusableBits.Ui.Platform;

namespace Noise.UI.ViewModels {
	public class FavoriteFilter {
		public string	FilterName { get; }
		public bool		FilterArtists { get; }
		public bool		FilterAlbums { get; }
		public bool		FilterTracks { get; }

		public FavoriteFilter( string name, bool filterArtists, bool filterAlbums, bool filterTracks ) {
			FilterName = name;
			FilterArtists = filterArtists;
			FilterAlbums = filterAlbums;
			FilterTracks = filterTracks;
		}
	}

	public class FavoritesViewModel : AutomaticPropertyBase, IDisposable,
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
		private readonly IPlatformDialogService	mDialogService;
        private readonly IPlayingItemHandler    mPlayingItemHandler;
        private readonly IPrefixedNameHandler   mPrefixedNameHandler;
        private readonly List<FavoriteFilter>	mFilterList; 
		private FavoriteViewNode				mSelectedNode;
		private ICollectionView					mFavoritesView;
		private	IDisposable						mArtistSubscription;
		private	IDisposable						mAlbumSubscription;
		private TaskHandler<IEnumerable<FavoriteViewNode>>		mTaskHandler; 
		private readonly SortableCollection<FavoriteViewNode>	mFavoritesList;

        public	IEventAggregator								EventAggregator => mEventAggregator;
        public	IList<FavoriteFilter>							FilterList => mFilterList;
		public	bool											IsListFiltered => !String.IsNullOrWhiteSpace( FilterText );
        public	SortableCollection<FavoriteViewNode>			FavoritesCollection => mFavoritesList;

		public	DelegateCommand									PlayRandom { get; }
		public	DelegateCommand									ExportFavorites { get; }
		public	DelegateCommand									ImportFavorites { get; }

		public FavoritesViewModel( IEventAggregator eventAggregator, IDatabaseInfo databaseInfo, IPlayCommand playCommand, IPlayQueue playQueue, IRandomTrackSelector trackSelector,
								   IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider, ISelectionState selectionState, IPrefixedNameHandler nameHandler,
								   IDataExchangeManager dataExchangeManager, IPlatformDialogService dialogService, IPlayingItemHandler playingItemHandler, IUiLog log ) {
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
            mPlayingItemHandler = playingItemHandler;
            mPrefixedNameHandler = nameHandler;

			mFavoritesList = new SortableCollection<FavoriteViewNode>();

			var defaultFilter = new FavoriteFilter( "All", true, true, true );
			mFilterList = new List<FavoriteFilter> { defaultFilter, 
													 new FavoriteFilter( "Artists", true, false, false ),
													 new FavoriteFilter( "Albums", false, true, false ),
													 new FavoriteFilter( "Tracks", false, false, true )};
			CurrentFilter = defaultFilter;

			PlayRandom = new DelegateCommand( OnPlayRandom, CanPlayRandom );
			ExportFavorites = new DelegateCommand( OnExportFavorites, CanExportFavorites );
			ImportFavorites = new DelegateCommand( OnImportFavorites );

			mEventAggregator.Subscribe( this );
			mArtistSubscription = selectionState.CurrentArtistChanged.Subscribe( OnArtistChanged );
			mAlbumSubscription = selectionState.CurrentAlbumChanged.Subscribe( OnAlbumChanged );
            mPlayingItemHandler.StartHandler( mFavoritesList );

			if( databaseInfo.IsOpen ) {
				LoadFavorites();
			}
		}

	    public FavoriteFilter CurrentFilter {
			get { return( Get( () => CurrentFilter )); }
			set {
				Set( () => CurrentFilter, value );

				FavoritesList.Refresh();
			}
		}

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

			if( node is FavoriteViewNode favoriteNode ) {
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
					RaisePropertyChanged( () => IsListFiltered );
	            } );
	        }
	    }
		
        public FavoriteViewNode SelectedNode {
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
               ( mSelectedNode?.Artist != null ) &&
               ( mSelectedNode.Artist.DbId != newArtist.DbId )) {
				mSelectedNode = null;

				RaisePropertyChanged( () => SelectedNode );
			}
		}

		private void OnAlbumChanged( DbAlbum newAlbum ) {
			if(( newAlbum != null ) &&
			   ( mSelectedNode?.Album != null ) &&
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

			set => mTaskHandler = value;
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

                retValue.ForEach( f => {
                        f.DisplayName = mPrefixedNameHandler.FormatPrefixedName( f.Title );
                        f.SortingName = mPrefixedNameHandler.FormatSortingName( f.Title );
                    });
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
			mFavoritesList.Sort( n => n.SortingName, ListSortDirection.Ascending );
			mFavoritesList.IsNotifying = true;
			mFavoritesList.Refresh();

            mPlayingItemHandler.UpdateList();

			ExportFavorites.RaiseCanExecuteChanged();
            PlayRandom.RaiseCanExecuteChanged();
		}

		private void ClearFavorites() {
			mFavoritesList.Clear();
			mSelectedNode = null;

			RaisePropertyChanged( () => SelectedNode );
            ExportFavorites.RaiseCanExecuteChanged();
			PlayRandom.RaiseCanExecuteChanged();
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

		private void OnPlayRandom() {
			mPlayQueue.Add( mTrackSelector.SelectTracksFromFavorites( track => !mPlayQueue.IsTrackQueued( track ), 10 ));
		}

		private bool CanPlayRandom() {
			return( mFavoritesList.Any());
		}

		private void OnExportFavorites() {
            if( mDialogService.SaveFileDialog( "Export Favorites", Constants.ExportFileExtension, "Export Files|*" + Constants.ExportFileExtension, out var fileName ) == true ) {
				mDataExchangeMgr.ExportFavorites( fileName );
			}
		}

		private bool CanExportFavorites() {
			return( mFavoritesList.Count > 0 );
		}

		private void OnImportFavorites() {
			GlobalCommands.ImportFavorites.Execute( null );
		}

        public void Dispose() {
            mArtistSubscription?.Dispose();
			mArtistSubscription = null;

            mAlbumSubscription?.Dispose();
			mAlbumSubscription = null;
        }
    }
}
