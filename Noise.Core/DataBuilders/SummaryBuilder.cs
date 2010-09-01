﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
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

		public void BuildSummaryData() {
			mStop = false;

			SummarizeArtists();
		}

		public void Stop() {
			mStop = true;
		}

		private void SummarizeArtists() {
			var databaseMgr = mContainer.Resolve<IDatabaseManager>();
			var database = databaseMgr.ReserveDatabase();

			if( database != null ) {
				try {
					var artists = from DbArtist artist in database.Database select artist;

					foreach( var artist in artists ) {
						mLog.LogInfo( string.Format( "Building summary data for: {0}", artist.Name ));

						var artistId = artist.DbId;
						var albums = from DbAlbum album in database.Database where album.Artist == artistId select album;
						var albumGenre = new Dictionary<string, int>();
						var albumCount = 0;
						var albumRating = 0;

						foreach( var album in albums ) {
							var albumId = album.DbId;
							var tracks = from DbTrack track in database.Database where track.Album == albumId select track;
							var years = new List<UInt32>();
							var trackGenre = new Dictionary<string, int>();
							var trackRating = 0;

							album.TrackCount = 0;

							foreach( var track in tracks ) {
								if(!years.Contains( track.PublishedYear )) {
									years.Add( track.PublishedYear );
								}

								AddGenre( trackGenre, track.CalculatedGenre );
								album.TrackCount++;
								trackRating += track.Rating;
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

							album.CalculatedRating = (Int16)( trackRating / album.TrackCount );
							albumRating += album.CalculatedRating;

							database.Store( album );
							albumCount++;

							if( mStop ) {
								break;
							}
						}

						artist.AlbumCount = (Int16)albumCount;
						artist.CalculatedGenre = DetermineTopGenre( albumGenre );
						artist.CalculatedRating = (Int16)( albumRating / albumCount );

						database.Store( artist );

						if( mStop ) {
							break;
						}
					}
				}
				catch( Exception ex ) {
					mLog.LogException( "Building summary data: ", ex );
				}
				finally {
					databaseMgr.FreeDatabase( database );
				}
			}
		}

		private static void AddGenre( Dictionary<string, int> genres, string genre ) {
			if(!String.IsNullOrWhiteSpace( genre )) {
				if( genres.ContainsKey( genre )) {
					genres[genre]++;
				}
				else {
					genres.Add( genre, 1 );
				}
			}
		}

		private static string DetermineTopGenre( Dictionary<string, int> genres ) {
			var retValue = "";

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
