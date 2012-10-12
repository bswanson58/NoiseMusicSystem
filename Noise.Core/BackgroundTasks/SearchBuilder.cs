using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.BackgroundTasks {
	[Export( typeof( IBackgroundTask ))]
	public class SearchBuilder : IBackgroundTask,
								 IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing> {
		private readonly IEventAggregator				mEventAggregator;
		private readonly ISearchProvider				mSearchProvider;
		private readonly IArtistProvider				mArtistProvider;
		private readonly IAlbumProvider					mAlbumProvider;
		private readonly ITrackProvider					mTrackProvider;
		private readonly IAssociatedItemListProvider	mAssociationProvider;
		private readonly ILyricProvider					mLyricProvider;
		private readonly ITextInfoProvider				mTextInfoProvider;
		private List<long>								mArtistList;
		private IEnumerator<long>						mArtistEnum;

		public SearchBuilder( IEventAggregator eventAggregator, IArtistProvider artistProvider, IAlbumProvider albumProvider,
							  ITrackProvider trackProvider, IAssociatedItemListProvider associatedItemListProvider,
							  ILyricProvider lyricProvider, ITextInfoProvider textInfoProvider, ISearchProvider searchProvider ) {
			mEventAggregator = eventAggregator;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mAssociationProvider = associatedItemListProvider;
			mLyricProvider = lyricProvider;
			mTextInfoProvider = textInfoProvider;
			mSearchProvider = searchProvider;

			mEventAggregator.Subscribe( this );
		}

		public string TaskId {
			get { return( "Task_SearchBuilder" ); }
		}

		public void Handle( Events.DatabaseOpened args ) {
			InitializeLists();
		}

		public void Handle( Events.DatabaseClosing args ) {
			mArtistList.Clear();
		}

		private void InitializeLists() {
			using( var artistList = mArtistProvider.GetArtistList()) {
				mArtistList = new List<long>( from DbArtist artist in artistList.List select artist.DbId );
				mArtistEnum = mArtistList.GetEnumerator();
			}
		}

		public void ExecuteTask() {
			var attempts = 10;

			while( attempts > 0 ) {
				var artist = NextArtist();

				if( artist != null ) {
					if( CheckSearchIndex( artist )) {
						break;
					}

					attempts--;
				}
			}
		}

		private DbArtist NextArtist() {
			DbArtist retValue = null;

			if(!mArtistEnum.MoveNext()) {
				InitializeLists();

				mArtistEnum.MoveNext();
			}

			if( mArtistEnum.Current != 0 ) {
				retValue = mArtistProvider.GetArtist( mArtistEnum.Current );
			}

			return( retValue );
		}

		private bool CheckSearchIndex( DbArtist artist ) {
			var retValue = false;
			var lastUpdate = mSearchProvider.DetermineTimeStamp( artist );

			if( lastUpdate.Ticks < artist.LastChangeTicks ) {
				BuildSearchIndex( artist );

				retValue = true;
			}

			return( retValue );
		}

		private void BuildSearchIndex( DbArtist artist ) {
			NoiseLogger.Current.LogMessage( String.Format( "Building search info for {0}", artist.Name ));

			try {
				using( var indexBuilder = mSearchProvider.CreateIndexBuilder( artist, false )) {
					indexBuilder.DeleteArtistSearchItems();
					indexBuilder.WriteTimeStamp();
					indexBuilder.AddSearchItem( eSearchItemType.Artist, artist.Name );

					IEnumerable<DbAssociatedItemList>	associatedItems;
					using( var associatedList = mAssociationProvider.GetAssociatedItemLists( artist.DbId )) {
						associatedItems = new List<DbAssociatedItemList>( from item in associatedList.List where item.IsContentAvailable select item );
					}
					foreach( var item in associatedItems ) {
						var itemType = eSearchItemType.Unknown;

						switch( item.ContentType ) {
							case ContentType.SimilarArtists:
								itemType = eSearchItemType.SimilarArtist;
								break;

							case ContentType.TopAlbums:
								itemType = eSearchItemType.TopAlbum;
								break;

							case ContentType.BandMembers:
								itemType = eSearchItemType.BandMember;
								break;
						}

						if( itemType != eSearchItemType.Unknown ) {
							indexBuilder.AddSearchItem( itemType, item.GetItems());
						}
					}

					var biography = mTextInfoProvider.GetArtistTextInfo( artist.DbId, ContentType.Biography );
					if( biography != null ) {
						indexBuilder.AddSearchItem( eSearchItemType.Biography, biography.Text );
					}

					using( var albumList = mAlbumProvider.GetAlbumList( artist )) {
						foreach( var album in albumList.List ) {
							indexBuilder.AddSearchItem( album, eSearchItemType.Album, album.Name );

							var infoList = mTextInfoProvider.GetAlbumTextInfo( album.DbId );
							if( infoList != null ) {
								foreach( var info in infoList ) {
									indexBuilder.AddSearchItem( album, eSearchItemType.TextInfo, info.Text );
								}
							}

							using( var trackList = mTrackProvider.GetTrackList( album )) {
								foreach( var track in trackList.List ) {
									indexBuilder.AddSearchItem( album, track, eSearchItemType.Track, track.Name );
								}
							}
						}
					}

					using( var lyricsList = mLyricProvider.GetLyricsForArtist( artist )) {
						foreach( var lyric in lyricsList.List ) {
							var track = mTrackProvider.GetTrack( lyric.TrackId );

							if( track != null ) {
								var album = mAlbumProvider.GetAlbumForTrack( track );

								if( album != null ) {
									indexBuilder.AddSearchItem( album, track, eSearchItemType.Lyrics, lyric.Lyrics  );
								}
							}
						}
					}
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - Building search data: ", ex );
			}
		}
	}
}
