using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Noise.Core.Support;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.BackgroundTasks {
	[Export( typeof( IBackgroundTask ))]
	public class SearchBuilder : IBackgroundTask, IRequireInitialization {
		private readonly IDatabaseManager	mDatabaseManager;
		private readonly IDataProvider		mDataProvider;
		private readonly ISearchProvider	mSearchProvider;
		private List<long>					mArtistList;
		private IEnumerator<long>			mArtistEnum;

		public SearchBuilder( ILifecycleManager lifecycleManager, IDatabaseManager databaseManager, IDataProvider dataProvider, ISearchProvider searchProvider ) {
			mDatabaseManager = databaseManager;
			mDataProvider = dataProvider;
			mSearchProvider = searchProvider;

			lifecycleManager.RegisterForInitialize( this );
		}

		public string TaskId {
			get { return( "Task_SearchBuilder" ); }
		}

		public void Initialize() {
			InitializeLists();
		}

		public void Shutdown() { }

		private void InitializeLists() {
			using( var artistList = mDataProvider.GetArtistList()) {
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
				retValue = mDataProvider.GetArtist( mArtistEnum.Current );
			}

			return( retValue );
		}

		private bool CheckSearchIndex( DbArtist artist ) {
			var retValue = false;
			var lastUpdate = mSearchProvider.DetermineTimeStamp( artist );

			if( lastUpdate.Ticks < artist.LastChangeTicks ) {
				BuildSearchIndex( mDatabaseManager, artist );

				retValue = true;
			}

			return( retValue );
		}

		private void BuildSearchIndex( IDatabaseManager databaseMgr, DbArtist artist ) {
			NoiseLogger.Current.LogMessage( String.Format( "Building search info for {0}", artist.Name ));

			var database = databaseMgr.ReserveDatabase();

			if( database != null ) {
				try {
					using( var indexBuilder = mSearchProvider.CreateIndexBuilder( artist, false )) {
						indexBuilder.DeleteArtistSearchItems();
						indexBuilder.WriteTimeStamp();

						var parms = database.Database.CreateParameters();

						parms["biography"] = ContentType.Biography;
						parms["discography"] = ContentType.Discography;
						parms["textInfo"] = ContentType.TextInfo;

						indexBuilder.AddSearchItem( eSearchItemType.Artist, artist.Name );

						parms["artistId"] = artist.DbId;
						var associatedItems = database.Database.ExecuteQuery( "SELECT DbAssociatedItemList WHERE Artist = @artistId AND IsContentAvailable", parms ).OfType<DbAssociatedItemList>();
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

						var biography = database.Database.ExecuteScalar( "SELECT DbTextInfo Where Artist = @artistId AND ContentType = @biography", parms ) as DbTextInfo;
						if( biography != null ) {
							indexBuilder.AddSearchItem( eSearchItemType.Biography, database.BlobStorage.RetrieveText( biography.DbId ));
						}

						var	albumList = database.Database.ExecuteQuery( "SELECT DbAlbum WHERE Artist = @artistId", parms ).OfType<DbAlbum>();
						foreach( var album in albumList ) {
							indexBuilder.AddSearchItem( album, eSearchItemType.Album, album.Name );

							parms["albumId"] = album.DbId;
							var infoList = database.Database.ExecuteQuery( "SELECT DbTextInfo WHERE Album = @albumId AND ContentType = @textInfo", parms ).OfType<DbTextInfo>();

							foreach( var info in infoList ) {
								indexBuilder.AddSearchItem( album, eSearchItemType.TextInfo, database.BlobStorage.RetrieveText( info.DbId ));
							}

							var trackList = database.Database.ExecuteQuery( "SELECT DbTrack WHERE Album = @albumId", parms ).OfType<DbTrack>();

							foreach( var track in trackList ) {
								indexBuilder.AddSearchItem( album, track, eSearchItemType.Track, track.Name );
							}
						}

						var lyricsList = database.Database.ExecuteQuery( "SELECT DbLyric WHERE ArtistId = @artistId", parms ).OfType<DbLyric>();
						foreach( var lyric in lyricsList ) {
							var track = mDataProvider.GetTrack( lyric.TrackId );

							if( track != null ) {
								var album = mDataProvider.GetAlbumForTrack( track );

								if( album != null ) {
									indexBuilder.AddSearchItem( album, track, eSearchItemType.Lyrics, lyric.Lyrics  );
								}
							}
						}
					}
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( "Exception - Building search data: ", ex );
				}
				finally {
					databaseMgr.FreeDatabase( database );
				}
			}
		}
	}
}
