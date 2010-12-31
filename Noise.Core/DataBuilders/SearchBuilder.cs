using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataBuilders {
	public class SearchBuilder : ISearchBuilder {
		private readonly IUnityContainer	mContainer;
		private readonly INoiseManager		mNoiseManager;
		private readonly ILog				mLog;
		private bool						mStopBuilding;

		public SearchBuilder( IUnityContainer container ) {
			mContainer = container;
			mNoiseManager = mContainer.Resolve<INoiseManager>();
			mLog = mContainer.Resolve<ILog>();
		}

		public void BuildSearchIndex( IEnumerable<DbArtist> aRtistList ) {
			mStopBuilding = false;

			var databaseMgr = mContainer.Resolve<IDatabaseManager>();
			var database = databaseMgr.ReserveDatabase();

			if( database != null ) {
				try {
					if( mNoiseManager.SearchProvider.StartIndexUpdate( true )) {
						var artistList = database.Database.ExecuteQuery( "SELECT DbArtist" ).OfType<DbArtist>();
						var parms = database.Database.CreateParameters();

						parms["discography"] = ContentType.Discography;
						parms["textInfo"] = ContentType.TextInfo;

						foreach( var artist in artistList ) {
							mLog.LogMessage( String.Format( "Building search info for {0}", artist.Name ));

							mNoiseManager.SearchProvider.AddSearchItem( artist, eSearchItemType.Artist, artist.Name );

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
									mNoiseManager.SearchProvider.AddSearchItem( artist, itemType, item.GetItems());
								}
							}

							var bioList = database.Database.ExecuteQuery( "SELECT DbTextInfo WHERE Artist = @artistId AND ContentType = @discography AND IsContentAvailable", parms ).OfType<DbTextInfo>();
							var biography = bioList.FirstOrDefault();
							if( biography != null ) {
								mNoiseManager.SearchProvider.AddSearchItem( artist, eSearchItemType.Biography, biography.Text );
							}

							var	albumList = database.Database.ExecuteQuery( "SELECT DbAlbum WHERE Artist = @artistId", parms ).OfType<DbAlbum>();
							foreach( var album in albumList ) {
								mNoiseManager.SearchProvider.AddSearchItem( artist, album, eSearchItemType.Album, album.Name );

								parms["albumId"] = album.DbId;
								var infoList = database.Database.ExecuteQuery( "SELECT DbTextInfo WHERE Album = @albumId AND ContentType = @textInfo", parms ).OfType<DbTextInfo>();

								foreach( var info in infoList ) {
									mNoiseManager.SearchProvider.AddSearchItem( artist, album, eSearchItemType.TextInfo, info.Text );
								}

								var trackList = database.Database.ExecuteQuery( "SELECT DbTrack WHERE Album = @albumId", parms ).OfType<DbTrack>();

								foreach( var track in trackList ) {
									mNoiseManager.SearchProvider.AddSearchItem( artist, album, track, eSearchItemType.Track, track.Name );
								}
							}

							if( mStopBuilding ) {
								break;
							}
						}
					}
					else {
						mLog.LogMessage( "SearchProvider Could not start index update." );
					}
				}
				catch( Exception ex ) {
					mLog.LogException( "Exception - Building search data: ", ex );
				}
				finally {
					databaseMgr.FreeDatabase( database );
					mNoiseManager.SearchProvider.EndIndexUpdate();
				}
			}
		}

		public void Stop() {
			mStopBuilding = true;
		}
	}
}
