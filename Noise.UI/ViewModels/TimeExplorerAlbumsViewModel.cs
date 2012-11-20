using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class UiTimeExplorerAlbum : AutomaticCommandBase {
		private readonly Action<UiTimeExplorerAlbum>	mOnPlay;

		public	DbArtist	Artist { get; private set; }
		public	DbAlbum		Album { get; private set; }

		public UiTimeExplorerAlbum( DbArtist artist, DbAlbum album, Action<UiTimeExplorerAlbum> onPlay ) {
			Artist = artist;
			Album = album;

			mOnPlay = onPlay;
		}

		public void Execute_Play() {
			if( mOnPlay != null ) {
				mOnPlay( this );
			}
		}
	}

	public class TimeExplorerAlbumsViewModel : IHandle<Events.TimeExplorerAlbumFocus>,
											   IHandle<Events.DatabaseClosing> {
		private readonly IEventAggregator			mEventAggregator;
		private readonly IArtistProvider			mArtistProvider;
		private readonly Dictionary<long, DbArtist>	mArtistList;
		private readonly SortableCollection<UiTimeExplorerAlbum>	mAlbumList; 
		private TaskHandler							mAlbumLoadingTaskHandler;

		public TimeExplorerAlbumsViewModel( IEventAggregator eventAggregator, IArtistProvider artistProvider ) {
			mEventAggregator = eventAggregator;
			mArtistProvider = artistProvider;

			mArtistList = new Dictionary<long, DbArtist>();
			mAlbumList = new SortableCollection<UiTimeExplorerAlbum>();

			mEventAggregator.Subscribe( this );
		}

		public void Handle( Events.TimeExplorerAlbumFocus args ) {
			mAlbumList.Clear();

			LoadAlbums( args.AlbumList );
		}
		
		public void Handle( Events.DatabaseClosing args ) {
			mArtistList.Clear();
			mAlbumList.Clear();
		}

		public BindableCollection<UiTimeExplorerAlbum> AlbumList {
			get{ return( mAlbumList ); }
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

						mAlbumList.Sort( album => album.Artist.Name, ListSortDirection.Ascending );
						mAlbumList.IsNotifying = true;
						mAlbumList.Refresh();
					},
					() => { },
					ex => NoiseLogger.Current.LogException( "TimeExplorerAlbumsViewModel:LoadAlbums", ex ));
		}

		private void OnAlbumPlay( UiTimeExplorerAlbum album ) {
			GlobalCommands.PlayAlbum.Execute( album.Album );
		}
	}
}
