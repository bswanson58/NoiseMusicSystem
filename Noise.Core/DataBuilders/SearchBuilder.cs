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
						var artistList = from DbArtist artist in database.Database select artist;

						foreach( var artist in artistList ) {
							mLog.LogMessage( String.Format( "Building search info for {0}", artist.Name ));

							mNoiseManager.SearchProvider.AddSearchItem( artist, eSearchItemType.Artist, artist.Name );

							var artistId = artist.DbId;
							var associatedItems = from DbAssociatedItemList itemList in database.Database 
												  where itemList.Artist == artistId && itemList.IsContentAvailable select itemList;
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

							var biography = ( from DbTextInfo text in database.Database 
											  where text.Artist == artistId & text.ContentType == ContentType.Discography && text.IsContentAvailable 
											  select text ).FirstOrDefault();
							if( biography != null ) {
								mNoiseManager.SearchProvider.AddSearchItem( artist, eSearchItemType.Biography, biography.Text );
							}

							var albumList = from DbAlbum album in database.Database where album.Artist == artistId select album;
							foreach( var album in albumList ) {
								mNoiseManager.SearchProvider.AddSearchItem( artist, album, eSearchItemType.Album, album.Name );

								var albumId = album.DbId;
								var infoList = from DbTextInfo text in database.Database
											   where text.Album == albumId && text.ContentType == ContentType.TextInfo select text;

								foreach( var info in infoList ) {
									mNoiseManager.SearchProvider.AddSearchItem( artist, album, eSearchItemType.TextInfo, info.Text );
								}

								var trackList = from DbTrack track in database.Database where track.Album == albumId select track;

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
