using System;
using System.Linq;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	public class DatabaseStatistics {
		private readonly IDatabaseManager	mDatabase;
		private readonly ILog				mLog;

		public	int		ArtistCount { get; protected set; }
		public	int		AlbumCount { get; protected set; }
		public	int		TrackCount { get; protected set; }

		public	int		FolderCount { get; protected set; }
		public	int		FileCount { get; protected set; }

		public DatabaseStatistics( IDatabaseManager database ) {
			mDatabase = database;
			mLog = new Log();
		}

		public void GatherStatistics() {
			try {
				var folders = from StorageFolder folder in mDatabase.Database select folder;
				FolderCount = folders.Count();

				var files = from StorageFile file in mDatabase.Database select file;
				FileCount = files.Count();

				var artists = from DbArtist artist in mDatabase.Database select artist;
				ArtistCount = artists.Count();

				var albums = from DbAlbum album in mDatabase.Database select album;
				AlbumCount = albums.Count();

				var tracks = from DbTrack track in mDatabase.Database select track;
				TrackCount = tracks.Count();
			}
			catch( Exception ex ) {
				mLog.LogException( "Building Summary Data.", ex );
			}
		}

		public override string ToString() {
			return( String.Format( "Database Statistics - Artists: {0}, Albums: {1}, Tracks: {2}, Folders: {3}, Files: {4}",
									ArtistCount, AlbumCount, TrackCount, FolderCount, FileCount ));
		}
	}
}
