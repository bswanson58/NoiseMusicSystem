using System;
using System.ComponentModel;
using Caliburn.Micro;
using CuttingEdge.Conditions;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Adapters;
using Noise.UI.Support;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class FavoritesViewModel : AutomaticCommandBase,
									  IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing>,
	IHandle<Events.ArtistUserUpdate>, IHandle<Events.AlbumUserUpdate>, IHandle<Events.TrackUserUpdate> {
		private readonly IEventAggregator		mEventAggregator;
		private readonly IArtistProvider		mArtistProvider;
		private readonly IAlbumProvider			mAlbumProvider;
		private readonly ITrackProvider			mTrackProvider;
		private readonly IDataExchangeManager	mDataExchangeMgr;
		private readonly IDialogService			mDialogService;
		private readonly SortableCollection<FavoriteViewNode>	mFavoritesList;

		public FavoritesViewModel( IEventAggregator eventAggregator, IDatabaseInfo databaseInfo,
								   IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider,
								   IDataExchangeManager dataExchangeManager, IDialogService dialogService ) {
			mEventAggregator = eventAggregator;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mDataExchangeMgr = dataExchangeManager;
			mDialogService = dialogService;

			mEventAggregator.Subscribe( this );

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

		public void Handle( Events.DatabaseOpened args ) {
			LoadFavorites();
		}

		public void Handle( Events.DatabaseClosing args ) {
			mFavoritesList.Clear();
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

		private void LoadFavorites() {
			try {
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

						if( album != null ) {
							var artist = mArtistProvider.GetArtistForAlbum( album );

							mFavoritesList.Add( new FavoriteViewNode( artist, album, track, PlayTrack, SelectTrack ));
						}
					}
				}

				mFavoritesList.Sort( SelectSortProperty, ListSortDirection.Ascending );

				RaiseCanExecuteChangedEvent( "CanExecute_ExportFavorites" );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "LoadFavorites:", ex );
			}
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

		public void Execute_ImportFavorites() {
			GlobalCommands.ImportFavorites.Execute( null );
		}
	}
}
