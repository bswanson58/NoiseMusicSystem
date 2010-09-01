using System;
using System.Linq;
using Microsoft.Practices.Unity;
using MusicBrainz;
using Noise.Core.Database;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataProviders {
	public class MusicBrainzProvider {
		private readonly IUnityContainer	mContainer;
		private readonly ILog				mLog;

		public MusicBrainzProvider( IUnityContainer container ) {
			mContainer = container;
			mLog = mContainer.Resolve<ILog>();
		}

		public void BuildMetaData() {
			var dbManager = mContainer.Resolve<IDatabaseManager>();
			var database = dbManager.ReserveDatabase();

			try {
				var artists = from DbArtist artist in database.Database select artist;

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
			catch( Exception ex ) {
				mLog.LogException( "Exception - MusicBrainzProvider:", ex );
			}
			finally {
				dbManager.FreeDatabase( database );
			}
		}
	}
}
