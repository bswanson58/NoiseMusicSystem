using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.BackgroundTasks {
	[Export( typeof( IBackgroundTask ))]
	public class DiscographyExplorer : IBackgroundTask {
		private IDatabaseManager	mDatabaseMgr;
		private List<long>			mArtistList;
		private IEnumerator<long>	mArtistEnum;

		public string TaskId {
			get { return( "Task_DiscographyExplorer" ); }
		}

		public bool Initialize( IUnityContainer container, IDatabaseManager databaseManager ) {
			mDatabaseMgr = databaseManager;

			InitializeLists();

			return( true );
		}

		private void InitializeLists() {
			var database = mDatabaseMgr.ReserveDatabase();

			try {
				mArtistList = new List<long>( from DbArtist artist in database.Database.ExecuteQuery( "SELECT DbArtist" ).OfType<DbArtist>() select artist.DbId );
				mArtistEnum = mArtistList.GetEnumerator();
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "", ex );
			}
			finally {
				mDatabaseMgr.FreeDatabase( database );
			}
		}

		private long NextArtist() {
			if(!mArtistEnum.MoveNext()) {
				InitializeLists();

				mArtistEnum.MoveNext();
			}

			return( mArtistEnum.Current );
		}

		public void ExecuteTask() {
			var database = mDatabaseMgr.ReserveDatabase();

			try {
				var artistId = NextArtist();

				if( artistId != 0 ) {
					var parms = database.Database.CreateParameters();
					parms["artistId"] = artistId;

					var discography = database.Database.ExecuteQuery( "SELECT DbDiscographyRelease WHERE Artist = @artistId", parms ).OfType<DbDiscographyRelease>();
					var uniqueList = ReduceList( discography );
					var albumCache = new DatabaseCache<DbAlbum>( from DbAlbum album in database.Database 
																 where album.Artist == artistId && album.PublishedYear == Constants.cUnknownYear select album );
					foreach( var release in uniqueList ) {
						var releaseTitle = release.Title;
						var	dbAlbum = albumCache.Find( album => album.Name.Equals( releaseTitle, StringComparison.CurrentCultureIgnoreCase ));

						if( dbAlbum != null ) {
							dbAlbum.PublishedYear = release.Year;

							database.Store( dbAlbum );

							NoiseLogger.Current.LogMessage( string.Format( "Updating Published year from discography: album '{0}', year: '{1}'", dbAlbum.Name, dbAlbum.PublishedYear ));
						}
					}
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - DiscographyExplorer:Task ", ex );
			}
			finally {
				mDatabaseMgr.FreeDatabase( database );
			}
		}

		private static IEnumerable<DbDiscographyRelease> ReduceList( IEnumerable<DbDiscographyRelease> list ) {
			var uniqueList = new Dictionary<string, DbDiscographyRelease>();

			foreach( var release in list ) {
				if(( release.ReleaseType == DiscographyReleaseType.Release ) &&
				   ( release.Year != 0 )) {
					if(!uniqueList.ContainsKey( release.Title )) {
						uniqueList.Add( release.Title, release );
					}
					else {
						var currentRelease = uniqueList[release.Title];

						if( release.Year < currentRelease.Year ) {
							uniqueList[release.Title] = currentRelease;
						}
					}
				}
			}

			return( uniqueList.Values.ToList());
		}

		public void Shutdown() {
		}
	}
}
