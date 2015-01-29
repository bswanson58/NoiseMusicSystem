using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using CuttingEdge.Conditions;
using Noise.Core.Logging;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.Core.DataBuilders {
	public class MetaDataCleaner : IMetaDataCleaner {
		private readonly ILogLibraryCleaning		mLog;
		private readonly IEventAggregator			mEventAggregator;
		private readonly IArtistProvider			mArtistProvider;
		private readonly IAlbumProvider				mAlbumProvider;
		private readonly ITrackProvider				mTrackProvider;
		private readonly IDbBaseProvider			mDbBaseProvider;
		private readonly IArtworkProvider			mArtworkProvider;
		private readonly ITextInfoProvider			mTextInfoProvider;
		private readonly IStorageFolderProvider		mStorageFolderProvider;
		private readonly IStorageFileProvider		mStorageFileProvider;
		private bool								mStopCleaning;
		private DatabaseChangeSummary				mSummary;
		private readonly List<long>					mAlbumList;

		public MetaDataCleaner( IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider, IDbBaseProvider dbBaseProvider,
								IArtworkProvider artworkProvider, ITextInfoProvider textInfoProvider,
								IStorageFolderProvider storageFolderProvider, IStorageFileProvider storageFileProvider,
								IEventAggregator eventAggregator, ILogLibraryCleaning log ) {
			mLog = log;
			mEventAggregator = eventAggregator;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mDbBaseProvider = dbBaseProvider;
			mArtworkProvider = artworkProvider;
			mTextInfoProvider = textInfoProvider;
			mStorageFolderProvider = storageFolderProvider;
			mStorageFileProvider = storageFileProvider;

			mAlbumList = new List<long>();
		}

		public void Stop() {
			mStopCleaning = true;
		}

		public void CleanDatabase( DatabaseChangeSummary summary ) {
			Condition.Requires( summary ).IsNotNull();

			mSummary = summary;
			mStopCleaning = false;
			mAlbumList.Clear();

			mLog.LogCleaningStarted();
			mEventAggregator.Publish( new Events.StatusEvent( "Starting Library Metadata cleaning." ));

			try {
				CleanFolders();
				CleanFiles();

				CleanArtists( CleanAlbums( mAlbumList ));
			}
			catch( Exception ex ) {
				mLog.LogCleaningException( "Cleaning library", ex );
			}

			mLog.LogCleaningCompleted();
			mEventAggregator.Publish( new Events.StatusEvent( "Finished Library Metadata cleaning." ));
		}

		private void CleanFolders() {
			using( var folderList = mStorageFolderProvider.GetDeletedFolderList()) {
				foreach( var folder in folderList.List ) {
					CleanFolder( folder );

					if( mStopCleaning ) {
						break;
					}
				}
			}
		}

		private void CleanFolder( StorageFolder folder ) {
			using( var childFolders = mStorageFolderProvider.GetChildFolders( folder.DbId )) {
				foreach( var childFolder in childFolders.List ) {
					CleanFolder( childFolder );
				}
			}

			CleanFolderFiles( folder );

			mLog.LogRemovingFolder( folder );
			mStorageFolderProvider.RemoveFolder( folder );
		}

		private void CleanFolderFiles( StorageFolder folder ) {
			using( var fileList = mStorageFileProvider.GetFilesInFolder( folder.DbId )) {
				foreach( var file in fileList.List ) {
					CleanFile( file );
				}
			}
		}

		private void CleanFiles() {
			using( var fileList = mStorageFileProvider.GetDeletedFilesList()) {
				foreach( var file in fileList.List ) {
					CleanFile( file );

					if( mStopCleaning ) {
						break;
					}
				}
			}
		}

		private void CleanFile( StorageFile file ) {
			if( file.MetaDataPointer != Constants.cDatabaseNullOid ) {
				var associatedItem = mDbBaseProvider.GetItem( file.MetaDataPointer );

				if( associatedItem != null ) {
					TypeSwitch.Do( associatedItem, TypeSwitch.Case<DbTrack>( CleanTrack),
												   TypeSwitch.Case<DbArtwork>( CleanArtwork ),
												   TypeSwitch.Case<DbTextInfo>( CleanTextInfo ));
				}
			}

			mLog.LogRemovingFile( file );
			mStorageFileProvider.DeleteFile( file );
		}

		private void CleanTrack( DbTrack track ) {
			if(!mAlbumList.Contains( track.Album )) {
				mAlbumList.Add( track.Album );
			}

			mTrackProvider.DeleteTrack( track );
			mLog.LogRemovingTrack( track );

			mSummary.TracksRemoved++;
		}

		private void CleanArtwork( DbArtwork artwork ) {
			if(!mAlbumList.Contains( artwork.Album )) {
				mAlbumList.Add( artwork.Album );
			}

			mArtworkProvider.DeleteArtwork( artwork );
			mLog.LogRemovingArtwork( artwork );
		}

		private void CleanTextInfo( DbTextInfo textInfo ) {
			if(!mAlbumList.Contains( textInfo.Album )) {
				mAlbumList.Add( textInfo.Album );
			}

			mTextInfoProvider.DeleteTextInfo( textInfo );
			mLog.LogRemovingTextInfo( textInfo );
		}

		private void CleanArtists( IEnumerable<long> artists ) {
			foreach( var artistId in artists ) {
				var artist = mArtistProvider.GetArtist( artistId );

				if( artist != null ) {
					using( var albumList = mAlbumProvider.GetAlbumList( artistId )) {
						if( !albumList.List.Any() ) {
							mArtistProvider.DeleteArtist( artist );

							mLog.LogRemovingArtist( artist );
							mEventAggregator.Publish( new Events.ArtistRemoved( artist.DbId ));

							mSummary.ArtistsRemoved++;
						}
					}
				}
			}
		}

		private IEnumerable<long> CleanAlbums( IEnumerable<long> albums ) {
			var retValue = new List<long>();

			foreach( var albumId in albums ) {
				var album = mAlbumProvider.GetAlbum( albumId );
				if( album != null ) {
					using( var trackList = mTrackProvider.GetTrackList( albumId )) {
						if( !trackList.List.Any() ) {
							if(!retValue.Contains( album.Artist )) {
								retValue.Add( album.Artist );
							}

							mAlbumProvider.DeleteAlbum( album );

							mLog.LogRemovingAlbum( album );
							mEventAggregator.Publish( new Events.AlbumRemoved( album.DbId ));

							mSummary.AlbumsRemoved++;
						}
					}

					mArtistProvider.UpdateArtistLastChanged( album.Artist );
				}
			}

			return( retValue );
		}
	}
}
