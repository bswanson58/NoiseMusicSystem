using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using Noise.Core.Logging;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.BackgroundTasks {
	[Export( typeof( IBackgroundTask ))]
	public class SearchBuilder : IBackgroundTask,
								 IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing> {
		private readonly IEventAggregator				mEventAggregator;
		private readonly ILogBackgroundTasks			mLog;
		private readonly ILogUserStatus					mUserStatus;
		private readonly ISearchProvider				mSearchProvider;
		private readonly IArtistProvider				mArtistProvider;
		private readonly IAlbumProvider					mAlbumProvider;
		private readonly ITrackProvider					mTrackProvider;
		private readonly ILyricProvider					mLyricProvider;
		private readonly ITextInfoProvider				mTextInfoProvider;
		private readonly IMetadataManager				mMetadataManager;
		private List<long>								mArtistList;
		private IEnumerator<long>						mArtistEnum;

		public SearchBuilder( IEventAggregator eventAggregator, IArtistProvider artistProvider, IAlbumProvider albumProvider,
							  ITrackProvider trackProvider, ILyricProvider lyricProvider, ITextInfoProvider textInfoProvider,
							  IMetadataManager metadataManager, ISearchProvider searchProvider,
							  ILogBackgroundTasks log, ILogUserStatus userStatus ) {
			mEventAggregator = eventAggregator;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mLyricProvider = lyricProvider;
			mTextInfoProvider = textInfoProvider;
			mMetadataManager = metadataManager;
			mSearchProvider = searchProvider;
			mLog = log;
			mUserStatus = userStatus;

			mEventAggregator.Subscribe( this );
		}

		public string TaskId {
			get { return( "Task_SearchBuilder" ); }
		}

		public void Handle( Events.DatabaseOpened args ) {
			InitializeLists();
		}

		public void Handle( Events.DatabaseClosing args ) {
			if( mArtistList != null ) {
				mArtistList.Clear();
			}
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
			mLog.StartingSearchBuilding( artist );

			try {
				using( var indexBuilder = mSearchProvider.CreateIndexBuilder( artist, false )) {
					indexBuilder.DeleteArtistSearchItems();
					indexBuilder.WriteTimeStamp();
					indexBuilder.AddSearchItem( eSearchItemType.Artist, artist.Name );

					var artistBio = mMetadataManager.GetArtistMetadata( artist.Name );

					indexBuilder.AddSearchItem( eSearchItemType.TopAlbum, artistBio.GetMetadataArray( eMetadataType.TopAlbums ));
					indexBuilder.AddSearchItem( eSearchItemType.BandMember,  artistBio.GetMetadataArray( eMetadataType.BandMembers ));
					indexBuilder.AddSearchItem( eSearchItemType.SimilarArtist, artistBio.GetMetadataArray( eMetadataType.SimilarArtists ));
					indexBuilder.AddSearchItem( eSearchItemType.Biography, artistBio.GetMetadata( eMetadataType.Biography ));

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

					mUserStatus.BuiltSearchData( artist );
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Building search data", ex );
			}

			mLog.CompletedSearchBuilding( artist );
		}
	}
}
