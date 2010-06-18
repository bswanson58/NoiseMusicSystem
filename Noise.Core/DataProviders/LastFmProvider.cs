using System;
using Lastfm.Services;
using Noise.Core.Database;

namespace Noise.Core.DataProviders {
	public class LastFmProvider {
		private readonly string				cApiKey		= "2cc6cebb071ba39a2d6fa71fc60255e8";
		private readonly string				cApiSecret	= "e01705ce5fa579cc070811ebfe5206f0";

		private readonly IDatabaseManager	mDatabase;

		public LastFmProvider( IDatabaseManager databaseManager ) {
			mDatabase = databaseManager;
		}

		public void BuildMetaData() {
			try {
				var session = new Session( cApiKey, cApiSecret );

//				var	artists = Lastfm.Services.Artist.Search( "AC-DC", session );
				var artist = new Artist( "AC-DC", session );
			}
			catch( Exception ex ) {
				
			}
		}
	}
}
