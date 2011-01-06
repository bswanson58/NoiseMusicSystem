using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Core.FileStore;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataBuilders {
	internal class SummaryBuilder : ISummaryBuilder {
		private readonly IUnityContainer	mContainer;
		private	readonly ILog				mLog;
		private bool						mStop;

		public SummaryBuilder( IUnityContainer container ) {
			mContainer =container;
			mLog = container.Resolve<ILog>();
		}

		public void BuildSummaryData( DatabaseChangeSummary summary ) {
			mStop = false;

			SummarizeArtists();
			UpdateSimilarArtists();
		}

		public void Stop() {
			mStop = true;
		}

		private void SummarizeArtists() {
			var databaseMgr = mContainer.Resolve<IDatabaseManager>();
			var database = databaseMgr.ReserveDatabase();

			if( database != null ) {
				try {
					var parms = database.Database.CreateParameters();
					var rootFolder = ( from RootFolder root in database.Database select  root ).FirstOrDefault();

					if( rootFolder != null ) {
						parms["lastScan"] = rootFolder.LastSummaryScan;
						var artistList = database.Database.ExecuteQuery( "SELECT DbArtist WHERE LastChangeTicks > @lastScan", parms ).OfType<DbArtist>();

						foreach( var artist in artistList ) {
							mLog.LogInfo( string.Format( "Building summary data for: {0}", artist.Name ));

							parms["artistId"] = artist.DbId;
							var	albums = database.Database.ExecuteQuery( "SELECT DbAlbum WHERE Artist = @artistId", parms ).OfType<DbAlbum>();
							var albumGenre = new Dictionary<long, int>();
							var albumCount = 0;
							var albumRating = 0;
							var maxAlbumRating = 0;

							foreach( var album in albums ) {
								var albumId = album.DbId;

								parms["albumId"] = albumId;
								var tracks = database.Database.ExecuteQuery( "SELECT DbTrack WHERE Album = @albumId", parms ).OfType<DbTrack>();
								var years = new List<UInt32>();
								var trackGenre = new Dictionary<long, int>();
								var trackRating = 0;
								var maxTrackRating = 0;

								album.TrackCount = 0;

								foreach( var track in tracks ) {
									if(!years.Contains( track.PublishedYear )) {
										years.Add( track.PublishedYear );
									}

									AddGenre( trackGenre, track.CalculatedGenre );
									album.TrackCount++;
									trackRating += track.Rating;

									if( track.Rating > maxTrackRating ) {
										maxTrackRating = track.Rating;
									}
								}

								if( years.Count == 0 ) {
									album.PublishedYear = Constants.cUnknownYear;
								}
								else if( years.Count == 1 ) {
									album.PublishedYear = years.First();
								}
								else {
									album.PublishedYear = Constants.cVariousYears;
								}

								album.CalculatedGenre = DetermineTopGenre( trackGenre );
								AddGenre( albumGenre, album.CalculatedGenre );

								album.CalculatedRating = trackRating > 0 ? (Int16)( trackRating / album.TrackCount ) : (Int16)0;
								album.MaxChildRating = (Int16)maxTrackRating;
								albumRating += album.CalculatedRating;
								if( maxTrackRating > maxAlbumRating ) {
									maxAlbumRating = maxTrackRating;
								}

								database.Store( album );
								albumCount++;

								if( mStop ) {
									break;
								}
							}

							artist.AlbumCount = (Int16)albumCount;
							artist.CalculatedGenre = DetermineTopGenre( albumGenre );
							artist.CalculatedRating = albumRating > 0 ? (Int16)( albumRating / albumCount ) : (Int16)0;
							artist.MaxChildRating = (Int16)maxAlbumRating;

							database.Store( artist );

							if( mStop ) {
								break;
							}
						}

						if(!mStop ) {
							rootFolder.UpdateSummaryScan();
							database.Store( rootFolder );
						}
					}
				}
				catch( Exception ex ) {
					mLog.LogException( "Exception - Building summary data: ", ex );
				}
				finally {
					databaseMgr.FreeDatabase( database );
				}
			}
		}

		private void UpdateSimilarArtists() {
			var databaseMgr = mContainer.Resolve<IDatabaseManager>();
			var database = databaseMgr.ReserveDatabase();

			if( database != null ) {
				try {
					mLog.LogInfo( "Building similar artist associations." );
					var artistCache = new DatabaseCache<DbArtist>( from DbArtist artist in database.Database select artist );
					var similarArtistLists = from DbAssociatedItemList list in database.Database where list.ContentType == ContentType.SimilarArtists select list;
					foreach( var similarArtistList in similarArtistLists ) {
						bool	needUpdate = false;

						foreach( var similarArtist in similarArtistList.Items ) {
							var artistName = similarArtist.Item;
							var dbArtist = artistCache.Find( artist => String.Compare( artist.Name, artistName, true ) == 0 );

							if( dbArtist != null ) {
								similarArtist.SetAssociatedId( dbArtist.DbId );

								needUpdate = true;
							}
						}

						if( needUpdate ) {
							database.Store( similarArtistList );
						}
					}
				}
				catch( Exception ex ) {
					mLog.LogException( "Exception - UpdateSimiliarArtists: ", ex );
				}
				finally {
					databaseMgr.FreeDatabase( database );
				}
			}
		}

		private static void AddGenre( Dictionary<long, int> genres, long genre ) {
			if( genre != Constants.cDatabaseNullOid ) {
				if( genres.ContainsKey( genre )) {
					genres[genre]++;
				}
				else {
					genres.Add( genre, 1 );
				}
			}
		}

		private static long DetermineTopGenre( Dictionary<long, int> genres ) {
			var retValue = Constants.cDatabaseNullOid;

			if( genres.Count > 0 ) {
				var genreCount = 0;

				foreach( var genre in genres.Keys ) {
					if( genres[genre] > genreCount ) {
						genreCount = genres[genre];
						retValue = genre;
					}
				}
			}

			return( retValue );
		}
	}
}
