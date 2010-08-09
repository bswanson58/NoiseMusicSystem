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
		private readonly IDatabaseManager	mDatabase;
		private	readonly ILog				mLog;

		public SummaryBuilder( IUnityContainer container ) {
			mDatabase = container.Resolve<IDatabaseManager>();
			mLog = container.Resolve<ILog>();
		}

		public void BuildSummaryData() {
			UpdateCounts( mDatabase );
		}

		private void UpdateCounts( IDatabaseManager database ) {
			try {
				var artists = from DbArtist artist in database.Database select artist;

				foreach( var artist in artists ) {
					mLog.LogInfo( string.Format( "Building summary data for: {0}", artist.Name ));

					var artistId = database.Database.GetUid( artist );
					var albums = from DbAlbum album in database.Database where album.Artist == artistId select album;
					var albumCount = 0;

					foreach( var album in albums ) {
						var albumId = database.Database.GetUid( album );
						var tracks = from DbTrack track in database.Database where track.Album == albumId select track;

						album.TrackCount = (Int16)tracks.Count();

						var years = new List<UInt32>();
						foreach( var track in tracks ) {
							if(!years.Contains( track.PublishedYear )) {
								years.Add( track.PublishedYear );
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

						database.Database.Store( album );
						albumCount++;
					}

					artist.AlbumCount = (Int16)albumCount;
					database.Database.Store( artist );
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Building summary data: ", ex );
			}
		}
	}
}
