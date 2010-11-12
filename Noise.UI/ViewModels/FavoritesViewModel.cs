using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;

namespace Noise.UI.ViewModels {
	public class FavoritesViewModel : ViewModelBase {
		private IUnityContainer			mContainer;
		private IEventAggregator		mEvents;
		private INoiseManager			mNoiseManager;
		private readonly ObservableCollectionEx<FavoriteViewNode>	mFavoritesList;

		public FavoritesViewModel() {
			mFavoritesList = new ObservableCollectionEx<FavoriteViewNode>();
		}

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;

				mEvents = mContainer.Resolve<IEventAggregator>();
				mNoiseManager = mContainer.Resolve<INoiseManager>();

				mEvents.GetEvent<Events.DatabaseItemChanged>().Subscribe( OnDatabaseChanged );

				LoadFavorites();
			}
		}

		public ObservableCollectionEx<FavoriteViewNode> FavoritesList {
			get{ return( mFavoritesList ); }
		}

		private void OnDatabaseChanged( DbItemChangedArgs args ) {
			if( args.Change == DbItemChanged.Favorite ) {
				var item = args.GetItem( mNoiseManager.DataProvider );

				if(( item is DbArtist ) ||
				   ( item is DbAlbum ) ||
				   ( item is DbTrack )) {
					BeginInvoke( LoadFavorites );
				}
			}
		}

		private void LoadFavorites() {
			mFavoritesList.SuspendNotification();
			mFavoritesList.Clear();

			using( var list = mNoiseManager.DataProvider.GetFavoriteArtists()) {
				foreach( var artist in list.List ) {
					mFavoritesList.Add( new FavoriteViewNode( artist, PlayArtist, SelectArtist ) );
				}
			}
			using( var list = mNoiseManager.DataProvider.GetFavoriteAlbums()) {
				foreach( var album in list.List ) {
					var artist = mNoiseManager.DataProvider.GetArtistForAlbum( album );

					mFavoritesList.Add( new FavoriteViewNode( artist, album, PlayAlbum, SelectAlbum ));
				}
			}
			using( var list = mNoiseManager.DataProvider.GetFavoriteTracks()) {
				foreach( var track in list.List ) {
					var album = mNoiseManager.DataProvider.GetAlbumForTrack( track );
					var artist = mNoiseManager.DataProvider.GetArtistForAlbum( album );

					mFavoritesList.Add( new FavoriteViewNode( artist, album, track, PlayTrack, SelectTrack ));
				}
			}

			mFavoritesList.ResumeNotification();
		}

		private void SelectArtist( FavoriteViewNode node ) {
			mEvents.GetEvent<Events.ArtistFocusRequested>().Publish( node.Artist );
		}

		private void SelectAlbum( FavoriteViewNode node ) {
			mEvents.GetEvent<Events.AlbumFocusRequested>().Publish( node.Album );
		}

		private void SelectTrack( FavoriteViewNode node ) {
			SelectAlbum( node );
		}

		private void PlayArtist( FavoriteViewNode node ) {
		}

		private void PlayAlbum( FavoriteViewNode node ) {
			mEvents.GetEvent<Events.AlbumPlayRequested>().Publish( node.Album );
		}

		private void PlayTrack( FavoriteViewNode node ) {
			mEvents.GetEvent<Events.TrackPlayRequested>().Publish( node.Track );
		}
	}
}
