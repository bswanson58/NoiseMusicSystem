using System.Linq;
using MusicBrainz;
using Noise.Core.Database;
using Noise.Infrastructure.Dto;

namespace Noise.Core.DataProviders {
	public class MusicBrainzProvider {
		private readonly IDatabaseManager	mDatabase;

		public MusicBrainzProvider( IDatabaseManager database ) {
			mDatabase = database;
		}

		public void BuildMetaData() {
			var artists = from DbArtist artist in mDatabase.Database select artist;

			foreach( var dbArtist in artists ) {
				var artQuery = Artist.Query( dbArtist.Name );
				var artist = artQuery.PerfectMatch();

				if( artist != null ) {
					var releases = artist.GetReleases();

					foreach( var release in releases ) {
						var title = release.GetTitle();
						var tracks = release.GetTracks();
					}
				}

				break;
			}
		}
	}
}
