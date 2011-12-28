using System;
using System.Collections.Generic;
using System.Linq;
using Noise.Core.FileStore;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataBuilders {
	internal class SummaryBuilder : ISummaryBuilder {
		private readonly IRootFolderProvider	mRootFolderProvider;
		private readonly IArtistProvider		mArtistProvider;
		private readonly IAlbumProvider			mAlbumProvider;
		private readonly ITrackProvider			mTrackProvider;
		private bool							mStop;

		public SummaryBuilder( IRootFolderProvider rootFolderProvider, IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider ) {
			mRootFolderProvider =rootFolderProvider;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
		}

		public void BuildSummaryData( DatabaseChangeSummary summary ) {
			mStop = false;

			SummarizeArtists();
		}

		public void Stop() {
			mStop = true;
		}

		private void SummarizeArtists() {
			try {
				RootFolder	rootFolder;
				using( var rootFolderList = mRootFolderProvider.GetRootFolderList()) {
					rootFolder = rootFolderList.List.FirstOrDefault();
				}

				if( rootFolder != null ) {
					using( var artistList = mArtistProvider.GetChangedArtists( rootFolder.LastSummaryScan )) {
						foreach( var artist in artistList.List ) {
							NoiseLogger.Current.LogInfo( string.Format( "Building summary data for: {0}", artist.Name ));

							using( var albumList = mAlbumProvider.GetAlbumList( artist.DbId )) {
								var albumGenre = new Dictionary<long, int>();
								var albumCount = 0;
								var albumRating = 0;
								var maxAlbumRating = 0;

								foreach( var album in albumList.List ) {
									using( var artistUpdater = mArtistProvider.GetArtistForUpdate( artist.DbId )) {
										using( var trackList = mTrackProvider.GetTrackList( album.DbId )) {
											using( var albumUpdater = mAlbumProvider.GetAlbumForUpdate( album.DbId )) {
												var years = new List<UInt32>();
												var trackGenre = new Dictionary<long, int>();
												var trackRating = 0;
												var maxTrackRating = 0;

												album.TrackCount = 0;

												foreach( var track in trackList.List ) {
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

												albumUpdater.Update();
												albumCount++;

												if( mStop ) {
													break;
												}
											}
										}

										artist.AlbumCount = (Int16)albumCount;
										artist.CalculatedGenre = DetermineTopGenre( albumGenre );
										artist.CalculatedRating = albumRating > 0 ? (Int16)( albumRating / albumCount ) : (Int16)0;
										artist.MaxChildRating = (Int16)maxAlbumRating;

										artistUpdater.Update();

										if( mStop ) {
											break;
										}
									}
								}
							}
						}
					}

					if(!mStop ) {
						using( var updater = mRootFolderProvider.GetFolderForUpdate( rootFolder.DbId )) {
							if( updater.Item != null ) {
								updater.Item.UpdateSummaryScan();

								updater.Update();
							}
						}
					}
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - Building summary data: ", ex );
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
