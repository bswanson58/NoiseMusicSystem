using System;
using System.Linq;
using Microsoft.Practices.Unity;
using Noise.Core.FileStore;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;

namespace Noise.Core.Database {
	public class DatabaseStatistics {
		private readonly IDatabaseManager	mDatabaseManager;
		private bool						mAllCounts;

		public	int		ArtistCount { get; protected set; }
		public	int		AlbumCount { get; protected set; }
		public	int		TrackCount { get; protected set; }

		public	int		FolderCount { get; protected set; }
		public	int		FileCount { get; protected set; }

		public	DateTime	LastScan { get; private set; }

		public DatabaseStatistics( IUnityContainer container ) {
			mDatabaseManager = container.Resolve<IDatabaseManager>();
		}

		public void GatherStatistics( bool allCounts ) {
			var database = mDatabaseManager.ReserveDatabase();
			try {
				ArtistCount = database.Database.ExecuteQuery( "SELECT DbArtist" ).OfType<DbArtist>().Count();
				AlbumCount = database.Database.ExecuteQuery( "SELECT DbAlbum" ).OfType<DbAlbum>().Count();

				if( allCounts ) {
					mAllCounts = true;

					FolderCount = database.Database.ExecuteQuery( "SELECT StorageFolder" ).OfType<StorageFolder>().Count();
					FileCount = database.Database.ExecuteQuery( "SELECT StorageFile" ).OfType<StorageFile>().Count();
					TrackCount = database.Database.ExecuteQuery( "SELECT DbTrack" ).OfType<DbTrack>().Count();
				}

				var rootFolder = database.Database.ExecuteQuery( "SELECT RootFolder" ).OfType<RootFolder>().FirstOrDefault();
				if( rootFolder != null ) {
					LastScan = new DateTime( rootFolder.LastLibraryScan );
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - Building Database Statistical Data.", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}
		}

		public override string ToString() {
			string	retValue;

			if( mAllCounts ) {
				retValue = String.Format( "Database Status: Last Scan - {0} {1} - Artists: {2}, Albums: {3}, Tracks: {4}, Folders: {5}, Files: {6}",
										   LastScan.ToShortDateString(), LastScan.ToShortTimeString(), ArtistCount, AlbumCount, TrackCount, FolderCount, FileCount );
			}
			else {
				retValue = String.Format( "Database Status: Last Scan - {0} {1} - Artists: {2}, Albums: {3}",
										   LastScan.ToShortDateString(), LastScan.ToShortTimeString(), ArtistCount, AlbumCount );			
			}

			return( retValue );
		}
	}
}
