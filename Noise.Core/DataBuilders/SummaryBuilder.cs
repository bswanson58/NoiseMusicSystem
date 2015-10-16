using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Noise.Core.Logging;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataBuilders {
	internal class SummaryBuilder : ISummaryBuilder {
		private readonly ILogLibraryBuildingSummary	mLog;
		private readonly IEventAggregator			mEventAggregator;
		private readonly ILogUserStatus				mUserStatus;
		private readonly IRootFolderProvider		mRootFolderProvider;
		private readonly IArtistProvider			mArtistProvider;
		private readonly IAlbumProvider				mAlbumProvider;
		private readonly ITrackProvider				mTrackProvider;
		private readonly IMetadataManager			mMetadataManager;
		private readonly ITagManager				mTagManager;
		private bool								mStop;

		public SummaryBuilder( IEventAggregator eventAggregator, IRootFolderProvider rootFolderProvider, IMetadataManager metadataManager, ITagManager tagManager,
							   IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider,
							   ILogLibraryBuildingSummary log, ILogUserStatus userStatus ) {
			mLog = log;
			mEventAggregator = eventAggregator;
			mUserStatus = userStatus;
			mRootFolderProvider =rootFolderProvider;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mMetadataManager = metadataManager;
			mTagManager = tagManager;
		}

		public void BuildSummaryData( DatabaseChangeSummary summary ) {
			mStop = false;

			mLog.LogSummaryBuildingStarted();
			mUserStatus.StartedLibrarySummary();
			SummarizeArtists();
			mLog.LogSummaryBuildingCompleted();
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
							mLog.LogSummaryArtistStarted( artist );

							using( var artistUpdater = mArtistProvider.GetArtistForUpdate( artist.DbId )) {
								var albumGenre = new Dictionary<long, int>();
								var maxAlbumRating = 0;
								var albumCount = 0;
								var albumRating = 0;

								using( var albumList = mAlbumProvider.GetAlbumList( artist.DbId )) {
									foreach( var album in albumList.List ) {
										mLog.LogSummaryAlbumStarted( album );

										using( var trackList = mTrackProvider.GetTrackList( album.DbId )) {
											using( var albumUpdater = mAlbumProvider.GetAlbumForUpdate( album.DbId )) {
												var years = new List<Int32>();
												var trackGenre = new Dictionary<long, int>();
												var trackRating = 0;
												var maxTrackRating = 0;

												albumUpdater.Item.TrackCount = 0;

												foreach( var track in trackList.List ) {
													if(!years.Contains( track.PublishedYear )) {
														years.Add( track.PublishedYear );
													}

													AddGenre( trackGenre, track.CalculatedGenre );
													albumUpdater.Item.TrackCount++;
													trackRating += track.Rating;

													if( track.Rating > maxTrackRating ) {
														maxTrackRating = track.Rating;
													}
												}

												// Don't overwrite the published year if it is already set.
												if( albumUpdater.Item.PublishedYear == Constants.cUnknownYear ) {
													if( years.Count == 0 ) {
														albumUpdater.Item.SetPublishedYear( Constants.cUnknownYear );
													}
													else if( years.Count == 1 ) {
														albumUpdater.Item.SetPublishedYear( years.First());
													}
													else {
														albumUpdater.Item.SetPublishedYear( Constants.cVariousYears );
													}

												}

												albumUpdater.Item.CalculatedGenre = DetermineTopGenre( trackGenre );
												AddGenre( albumGenre, albumUpdater.Item.CalculatedGenre );

												albumUpdater.Item.CalculatedRating = trackRating > 0 ? (Int16)( trackRating / albumUpdater.Item.TrackCount ) : (Int16)0;
												albumUpdater.Item.MaxChildRating = (Int16)maxTrackRating;
												albumRating += albumUpdater.Item.CalculatedRating;
												if( maxTrackRating > maxAlbumRating ) {
													maxAlbumRating = maxTrackRating;
												}

												mLog.LogSummaryAlbumCompleted( albumUpdater.Item );
												albumUpdater.Update();
												albumCount++;

												if( mStop ) {
													break;
												}
											}
										}
									}
								}

								artistUpdater.Item.AlbumCount = (Int16)albumCount;
								artistUpdater.Item.CalculatedGenre = DetermineTopGenre( albumGenre );
								artistUpdater.Item.CalculatedRating = albumRating > 0 ? (Int16)( albumRating / albumCount ) : (Int16)0;
								artistUpdater.Item.MaxChildRating = (Int16)maxAlbumRating;

								var artistMetadata = mMetadataManager.GetArtistMetadata( artistUpdater.Item.Name );
								var genre = artistMetadata.GetMetadataArray( eMetadataType.Genre ).FirstOrDefault();

								if(!string.IsNullOrWhiteSpace( genre )) {
									artistUpdater.Item.ExternalGenre = mTagManager.ResolveGenre( genre );
								}

								mLog.LogSummaryArtistCompleted( artistUpdater.Item );
								artistUpdater.Update();
								mEventAggregator.Publish( new Events.ArtistContentUpdated( artistUpdater.Item.DbId ));

								if( mStop ) {
									break;
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
				mLog.LogSummaryBuildingException( "Summary building", ex );
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
