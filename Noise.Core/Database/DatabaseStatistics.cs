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
				FolderCount = folders.Select( folder => folder.Name ).Count();

				var files = from StorageFile file in mDatabase.Database select file;
				FileCount = files.Select( file => file.Name ).Count();

				var artists = from DbArtist artist in mDatabase.Database select artist;
				ArtistCount = artists.Select( artist => artist.Name ).Count();

				var albums = from DbAlbum album in mDatabase.Database select album;
				AlbumCount = albums.Select( album => album.Name ).Count();

				var tracks = from DbTrack track in mDatabase.Database select track;
				TrackCount = tracks.Select( track => track.Name ).Count();
			}
			catch( Exception ex ) {
				mLog.LogException( "Building Summary Data.", ex );
			}
		}
	}
}
