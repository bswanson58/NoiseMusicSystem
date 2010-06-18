using System;
using System.Linq;
using Lastfm.Services;
using Noise.Core.Database;
using Noise.Core.MetaData;

namespace Noise.Core.DataProviders {
	public class LastFmProvider {
		private const string		cApiKey		= "2cc6cebb071ba39a2d6fa71fc60255e8";
		private const string		cApiSecret	= "e01705ce5fa579cc070811ebfe5206f0";
		private const int			cMaximumQueries = 5;

		private readonly IDatabaseManager	mDatabase;

		public LastFmProvider( IDatabaseManager databaseManager ) {
			mDatabase = databaseManager;
		}

		public void BuildMetaData() {
			try {
				var session = new Session( cApiKey, cApiSecret );

				UpdateArtistBios( session );
			}
			catch( Exception ex ) {
				
			}
		}

		private void UpdateArtistBios( Session session ) {
			var artists = from DbArtist artist in mDatabase.Database select artist;
			var queryCount = 0;

			foreach( var artist in artists ) {
				try {
					var parm = mDatabase.Database.CreateParameters();

					parm["artist"] = mDatabase.Database.GetUid( artist );
					var bio = mDatabase.Database.ExecuteScalar( "SELECT DbBiography WHERE Artist = @artist", parm ) as DbBiography;

					if( bio == null ) {
						var	artistSearch = Artist.Search( artist.Name, session );
						var	artistMatch = artistSearch.GetFirstMatch();

						if( artistMatch != null ) {
							bio = new DbBiography( mDatabase.Database.GetUid( artist ));

							bio.Biography = artistMatch.Bio.getContent();
							bio.PublishedDate = artistMatch.Bio.GetPublishedDate();
							bio.ExpireDate = DateTime.Now.Date + new TimeSpan( 30, 0, 0, 0 );

							mDatabase.Database.Store( bio );

							queryCount++;
							if( queryCount > cMaximumQueries ) {
								break;
							}
						}
					}
				}
				catch( Exception ex ) {
					
				}
			}
		}
	}
}
