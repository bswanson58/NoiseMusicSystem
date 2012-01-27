using System.ComponentModel;
using Caliburn.Micro;
using CuttingEdge.Conditions;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;
using Noise.UI.Support;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class FavoritesViewModel : ViewModelBase, IHandle<Events.DatabaseItemChanged> {
		private readonly IEventAggregator		mEventAggregator;
		private readonly IArtistProvider		mArtistProvider;
		private readonly IAlbumProvider			mAlbumProvider;
		private readonly ITrackProvider			mTrackProvider;
		private readonly IDataExchangeManager	mDataExchangeMgr;
		private readonly IDialogService			mDialogService;
		private readonly ObservableCollectionEx<FavoriteViewNode>	mFavoritesList;

		public FavoritesViewModel( IEventAggregator eventAggregator,
								   IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider,
								   IDataExchangeManager dataExchangeManager, IDialogService dialogService ) {
			mEventAggregator = eventAggregator;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mDataExchangeMgr = dataExchangeManager;
			mDialogService = dialogService;

			mEventAggregator.Subscribe( this );

			mFavoritesList = new ObservableCollectionEx<FavoriteViewNode>();
			LoadFavorites();
		}

		public ObservableCollectionEx<FavoriteViewNode> FavoritesList {
			get{ return( mFavoritesList ); }
		}

		public void Handle( Events.DatabaseItemChanged eventArgs ) {
			if( eventArgs.ItemChangedArgs.Change == DbItemChanged.Favorite ) {
				var item = eventArgs.ItemChangedArgs.Item;

				if(( item is DbArtist ) ||
				   ( item is DbAlbum ) ||
				   ( item is DbTrack )) {
					Execute.OnUIThread( LoadFavorites );
				}
			}
		}

		private void LoadFavorites() {
			mFavoritesList.SuspendNotification();
			mFavoritesList.Clear();

			using( var list = mArtistProvider.GetFavoriteArtists()) {
				foreach( var artist in list.List ) {
					mFavoritesList.Add( new FavoriteViewNode( artist, PlayArtist, SelectArtist ) );
				}
			}
			using( var list = mAlbumProvider.GetFavoriteAlbums()) {
				foreach( var album in list.List ) {
					var artist = mArtistProvider.GetArtistForAlbum( album );

					mFavoritesList.Add( new FavoriteViewNode( artist, album, PlayAlbum, SelectAlbum ));
				}
			}
			using( var list = mTrackProvider.GetFavoriteTracks()) {
				foreach( var track in list.List ) {
					var album = mAlbumProvider.GetAlbumForTrack( track );
					var artist = mArtistProvider.GetArtistForAlbum( album );

					mFavoritesList.Add( new FavoriteViewNode( artist, album, track, PlayTrack, SelectTrack ));
				}
			}

			mFavoritesList.Sort( SelectSortProperty, ListSortDirection.Ascending );
			mFavoritesList.ResumeNotification();

			RaiseCanExecuteChangedEvent( "CanExecute_ExportFavorites" );
		}

		private static string SelectSortProperty( FavoriteViewNode node ) {
			Condition.Requires( node ).IsNotNull();

			return( node.Track != null ? node.Track.Name : node.Album != null ? node.Album.Name : node.Artist != null ? node.Artist.Name : string.Empty );
		}

		private void SelectArtist( FavoriteViewNode node ) {
			mEventAggregator.Publish( new Events.ArtistFocusRequested( node.Artist.DbId ));
		}

		private void SelectAlbum( FavoriteViewNode node ) {
			if( node.Album != null ) {
				mEventAggregator.Publish( new Events.AlbumFocusRequested( node.Album ));
			}
		}

		private void SelectTrack( FavoriteViewNode node ) {
			SelectAlbum( node );
		}

		private static void PlayArtist( FavoriteViewNode node ) {
		}

		private static void PlayAlbum( FavoriteViewNode node ) {
			GlobalCommands.PlayAlbum.Execute( node.Album );
		}

		private static void PlayTrack( FavoriteViewNode node ) {
			GlobalCommands.PlayTrack.Execute( node.Track );
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
	}
}
