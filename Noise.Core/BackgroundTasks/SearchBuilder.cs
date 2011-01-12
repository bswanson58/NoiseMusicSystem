using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.BackgroundTasks {
	[Export( typeof( IBackgroundTask ))]
	public class SearchBuilder : IBackgroundTask {
		private IUnityContainer		mContainer;
		private INoiseManager		mNoiseManager;
		private ILog				mLog;
		private List<long>			mArtistList;
		private IEnumerator<long>	mArtistEnum;

		public string TaskId {
			get { return( "Task_SearchBuilder" ); }
		}

		public bool Initialize( IUnityContainer container ) {
			mContainer = container;
			mNoiseManager = mContainer.Resolve<INoiseManager>();
			mLog = mContainer.Resolve<ILog>();

			InitializeLists();

			return( true );
		}

		private void InitializeLists() {
			using( var artistList = mNoiseManager.DataProvider.GetArtistList()) {
				mArtistList = new List<long>( from DbArtist artist in artistList.List select artist.DbId );
				mArtistEnum = mArtistList.GetEnumerator();
			}
		}

		public void ExecuteTask() {
			var artist = NextArtist();

			if( artist != null ) {
				CheckSearchIndex( artist );
			}
		}

		private DbArtist NextArtist() {
			DbArtist retValue = null;

			if(!mArtistEnum.MoveNext()) {
				InitializeLists();

				mArtistEnum.MoveNext();
			}

			if( mArtistEnum.Current != 0 ) {
				retValue = mNoiseManager.DataProvider.GetArtist( mArtistEnum.Current );
			}

			return( retValue );
		}

		private void CheckSearchIndex( DbArtist artist ) {
			var lastUpdate = mNoiseManager.SearchProvider.DetermineTimeStamp( artist );

			if( lastUpdate.Ticks < artist.LastChangeTicks ) {
				BuildSearchIndex( artist );
			}
		}

		private void BuildSearchIndex( DbArtist artist ) {
			mLog.LogMessage( String.Format( "Building search info for {0}", artist.Name ));

			var databaseMgr = mContainer.Resolve<IDatabaseManager>();
			var database = databaseMgr.ReserveDatabase();

			if( database != null ) {
				try {
					using( var indexBuilder = mNoiseManager.SearchProvider.CreateIndexBuilder( artist, false )) {
						indexBuilder.DeleteArtistSearchItems();
						indexBuilder.WriteTimeStamp();

						var parms = database.Database.CreateParameters();

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

						var bioList = database.Database.ExecuteQuery( "SELECT DbTextInfo WHERE Artist = @artistId AND ContentType = @discography AND IsContentAvailable", parms ).OfType<DbTextInfo>();
						var biography = bioList.FirstOrDefault();
						if( biography != null ) {
							indexBuilder.AddSearchItem( eSearchItemType.Biography, biography.Text );
						}

						var	albumList = database.Database.ExecuteQuery( "SELECT DbAlbum WHERE Artist = @artistId", parms ).OfType<DbAlbum>();
						foreach( var album in albumList ) {
							indexBuilder.AddSearchItem( album, eSearchItemType.Album, album.Name );

							parms["albumId"] = album.DbId;
							var infoList = database.Database.ExecuteQuery( "SELECT DbTextInfo WHERE Album = @albumId AND ContentType = @textInfo", parms ).OfType<DbTextInfo>();

							foreach( var info in infoList ) {
								indexBuilder.AddSearchItem( album, eSearchItemType.TextInfo, info.Text );
							}

							var trackList = database.Database.ExecuteQuery( "SELECT DbTrack WHERE Album = @albumId", parms ).OfType<DbTrack>();

							foreach( var track in trackList ) {
								indexBuilder.AddSearchItem( album, track, eSearchItemType.Track, track.Name );
							}
						}
					}
				}
				catch( Exception ex ) {
					mLog.LogException( "Exception - Building search data: ", ex );
				}
				finally {
					databaseMgr.FreeDatabase( database );
				}
			}
		}

		public void Shutdown() {
		}
	}
}
