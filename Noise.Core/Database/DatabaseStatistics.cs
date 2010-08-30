using System;
using System.Linq;
using Microsoft.Practices.Unity;
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

		public DatabaseStatistics( IUnityContainer container ) {
			mDatabase = container.Resolve<IDatabaseManager>();
			mLog = container.Resolve<ILog>();
		}

		public void GatherStatistics() {
			try {
				if( mDatabase.InitializeAndOpenDatabase( "DatabaseStatistics" )) {
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

					mDatabase.CloseDatabase( "DatabaseStatistics" );
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Building Database Statistical Data.", ex );
			}
		}

		public override string ToString() {
			return( String.Format( "Database Statistics - Artists: {0}, Albums: {1}, Tracks: {2}, Folders: {3}, Files: {4}",
									ArtistCount, AlbumCount, TrackCount, FolderCount, FileCount ));
		}
	}
}
