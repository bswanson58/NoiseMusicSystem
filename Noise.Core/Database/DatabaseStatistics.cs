using System;
using System.Linq;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	public class DatabaseStatistics {
		private readonly IDatabaseManager	mDatabaseManager;
		private readonly ILog				mLog;

		public	int		ArtistCount { get; protected set; }
		public	int		AlbumCount { get; protected set; }
		public	int		TrackCount { get; protected set; }

		public	int		FolderCount { get; protected set; }
		public	int		FileCount { get; protected set; }

		public DatabaseStatistics( IUnityContainer container ) {
			mDatabaseManager = container.Resolve<IDatabaseManager>();
			mLog = container.Resolve<ILog>();
		}

		public void GatherStatistics() {
			var database = mDatabaseManager.ReserveDatabase();
			try {
				var folders = from StorageFolder folder in database.Database select folder;
				FolderCount = folders.Count();

				var files = from StorageFile file in database.Database select file;
				FileCount = files.Count();

				var artists = from DbArtist artist in database.Database select artist;
				ArtistCount = artists.Count();

				var albums = from DbAlbum album in database.Database select album;
				AlbumCount = albums.Count();

				var tracks = from DbTrack track in database.Database select track;
				TrackCount = tracks.Count();
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - Building Database Statistical Data.", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}
		}

		public override string ToString() {
			return( String.Format( "Database Statistics - Artists: {0}, Albums: {1}, Tracks: {2}, Folders: {3}, Files: {4}",
									ArtistCount, AlbumCount, TrackCount, FolderCount, FileCount ));
		}
	}
}
