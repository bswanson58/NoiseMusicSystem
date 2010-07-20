using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;

namespace Noise.Core.DataBuilders {
	internal class SummaryBuilder : ISummaryBuilder {
		private readonly IDatabaseManager	mDatabase;

		public SummaryBuilder( IUnityContainer container ) {
			mDatabase = container.Resolve<IDatabaseManager>();
		}

		public void BuildSummaryData() {
			UpdateCounts( mDatabase );
		}

		private static void UpdateCounts( IDatabaseManager database ) {
			var artists = from DbArtist artist in database.Database select artist;

			foreach( var artist in artists ) {
				var artistId = database.Database.GetUid( artist );
				var albums = from DbAlbum album in database.Database where album.Artist == artistId select album;

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
				}

				artist.AlbumCount = (Int16)albums.Count();
				database.Database.Store( artist );
			}
		}
	}
}
