using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Dto;
using Noise.UI.Logging;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	internal class TimeExplorerAlbumsViewModel : AutomaticCommandBase,
												 IHandle<Events.TimeExplorerAlbumFocus>,
												 IHandle<Events.DatabaseClosing> {
		private readonly IEventAggregator			mEventAggregator;
		private readonly IUiLog						mLog;
		private readonly IArtistProvider			mArtistProvider;
		private readonly Dictionary<long, DbArtist>	mArtistList;
		private readonly SortableCollection<UiTimeExplorerAlbum>	mAlbumList; 
		private TaskHandler							mAlbumLoadingTaskHandler;

		public TimeExplorerAlbumsViewModel( IEventAggregator eventAggregator, IArtistProvider artistProvider, IUiLog log ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mArtistProvider = artistProvider;

			mArtistList = new Dictionary<long, DbArtist>();
			mAlbumList = new SortableCollection<UiTimeExplorerAlbum>();

			mEventAggregator.Subscribe( this );
		}

		public void Handle( Events.TimeExplorerAlbumFocus args ) {
			mAlbumList.Clear();

			LoadAlbums( args.AlbumList );

			mEventAggregator.Publish( new Events.ArtistFocusRequested( Constants.cDatabaseNullOid ));
		}
		
		public void Handle( Events.DatabaseClosing args ) {
			mArtistList.Clear();
			mAlbumList.Clear();
		}

		public BindableCollection<UiTimeExplorerAlbum> AlbumList {
			get{ return( mAlbumList ); }
		}
 
		public UiTimeExplorerAlbum SelectedAlbum {
			get{ return( Get( () => SelectedAlbum )); }
			set {
				Set( () => SelectedAlbum, value );

				if( value != null ) {
					mEventAggregator.Publish( new Events.ArtistFocusRequested( value.Artist.DbId ));
					mEventAggregator.Publish( new Events.AlbumFocusRequested( value.Artist.DbId, value.Album.DbId ));
				}
			}
		}

		internal TaskHandler AlbumLoaderTask {
			get {
				if( mAlbumLoadingTaskHandler == null ) {
					Execute.OnUIThread( () => mAlbumLoadingTaskHandler = new TaskHandler());
				}

				return( mAlbumLoadingTaskHandler );
			}

			set{ mAlbumLoadingTaskHandler = value; }
		}

		private void LoadAlbums( IEnumerable<DbAlbum>  albumList ) {
			AlbumLoaderTask.StartTask( () => {
						if(!mArtistList.Any()) {
							using( var artistList = mArtistProvider.GetArtistList()) {
								foreach( var artist in artistList.List ) {
									mArtistList.Add( artist.DbId, artist );
								}
							}
						}

						mAlbumList.IsNotifying = false;

						foreach( var album in albumList ) {
							if( mArtistList.ContainsKey( album.Artist )) {
								var artist = mArtistList[album.Artist];

								mAlbumList.Add( new UiTimeExplorerAlbum( artist, album, OnAlbumPlay ));
							}
						}

						mAlbumList.Sort( album => album.Artist.Name + album.Album.Name, ListSortDirection.Ascending );
						mAlbumList.IsNotifying = true;
						mAlbumList.Refresh();
					},
					() => { },
					ex => mLog.LogException( "Loading Albums", ex ));
		}

		private void OnAlbumPlay( UiTimeExplorerAlbum album ) {
			GlobalCommands.PlayAlbum.Execute( album.Album );
		}
	}
}
