using System;
using System.Linq;
using Noise.Core.Logging;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	public class DatabaseStatistics : IDatabaseStatistics {
		private readonly ILogLibraryBuilding	mLog;
		private readonly IArtistProvider		mArtistProvider;
		private readonly IAlbumProvider			mAlbumProvider;
		private readonly ITrackProvider			mTrackProvider;
		private readonly IRootFolderProvider	mRootFolderProvider;
		private readonly IStorageFolderProvider	mStorageFolderProvider;
		private readonly IStorageFileProvider	mStorageFileProvider;

		private bool						mAllCounts;

		public	long	ArtistCount { get; protected set; }
		public	long	AlbumCount { get; protected set; }
		public	long	TrackCount { get; protected set; }

		public	long	FolderCount { get; protected set; }
		public	long	FileCount { get; protected set; }

		public	DateTime	LastScan { get; private set; }

		public DatabaseStatistics( IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider,
								   IRootFolderProvider rootFolderProvider, IStorageFolderProvider storageFolderProvider, IStorageFileProvider storageFileProvider,
								   ILogLibraryBuilding log ) {
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mRootFolderProvider = rootFolderProvider;
			mStorageFolderProvider = storageFolderProvider;
			mStorageFileProvider = storageFileProvider;
			mLog = log;
		}

		public void GatherStatistics( bool allCounts ) {
			try {
				ArtistCount = mArtistProvider.GetItemCount();
				AlbumCount = mAlbumProvider.GetItemCount();

				if( allCounts ) {
					mAllCounts = true;

					FolderCount = mStorageFolderProvider.GetItemCount();
					FileCount = mStorageFileProvider.GetItemCount();
					TrackCount = mTrackProvider.GetItemCount();
				}

				using( var rootFolderList = mRootFolderProvider.GetRootFolderList()) {
					var rootFolder = rootFolderList.List.FirstOrDefault();

					if( rootFolder != null ) {
						LastScan = new DateTime( rootFolder.LastLibraryScan );
					}
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Gethering statistics", ex );
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
