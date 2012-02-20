using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using CuttingEdge.Conditions;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.Core.DataBuilders {
	public class MetaDataCleaner : IMetaDataCleaner {
		private readonly IEventAggregator			mEventAggregator;
		private readonly IArtistProvider			mArtistProvider;
		private readonly IAlbumProvider				mAlbumProvider;
		private readonly ITrackProvider				mTrackProvider;
		private readonly IDbBaseProvider			mDbBaseProvider;
		private readonly IExpiringContentProvider	mContentProvider;
		private readonly IArtworkProvider			mArtworkProvider;
		private readonly ITextInfoProvider			mTextInfoProvider;
		private readonly IStorageFolderProvider		mStorageFolderProvider;
		private readonly IStorageFileProvider		mStorageFileProvider;
		private bool								mStopCleaning;
		private DatabaseChangeSummary				mSummary;
		private readonly List<long>					mAlbumList;

		public MetaDataCleaner( IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider, IDbBaseProvider dbBaseProvider,
								IArtworkProvider artworkProvider, ITextInfoProvider textInfoProvider, IExpiringContentProvider contentProvider,
								IStorageFolderProvider storageFolderProvider, IStorageFileProvider storageFileProvider, IEventAggregator eventAggregator ) {
			mEventAggregator = eventAggregator;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mDbBaseProvider = dbBaseProvider;
			mArtworkProvider = artworkProvider;
			mTextInfoProvider = textInfoProvider;
			mContentProvider = contentProvider;
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

			NoiseLogger.Current.LogMessage( "Starting MetaDataCleaning." );

			try {
				CleanFolders();
				CleanFiles();

				CleanArtists( CleanAlbums( mAlbumList ));
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - MetaDataCleaner:", ex );
			}
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

			NoiseLogger.Current.LogMessage( string.Format( "Deleting Folder: {0}", folder.Name ));

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
												   TypeSwitch.Case<DbArtwork>( CleanContent ),
												   TypeSwitch.Case<DbTextInfo>( CleanContent ));

					if( associatedItem is DbArtwork ) {
						mArtworkProvider.DeleteArtwork( associatedItem as DbArtwork );
					}
					if( associatedItem is DbTextInfo ) {
						mTextInfoProvider.DeleteTextInfo( associatedItem as DbTextInfo );
					}
				}
			}

			mStorageFileProvider.DeleteFile( file );
		}

		private void CleanTrack( DbTrack track ) {
			if(!mAlbumList.Contains( track.Album )) {
				mAlbumList.Add( track.Album );
			}

			mSummary.TracksRemoved++;
			NoiseLogger.Current.LogMessage( "Deleting Track: {0}", track.Name );
		}

		private void CleanContent( ExpiringContent content ) {
			if(!mAlbumList.Contains( content.Album )) {
				mAlbumList.Add( content.Album );
			}
		}

		private void CleanArtists( IEnumerable<long> artists ) {
			foreach( var artistId in artists ) {
				var artist = mArtistProvider.GetArtist( artistId );

				if( artist != null ) {
					using( var albumList = mAlbumProvider.GetAlbumList( artistId )) {
						if( !albumList.List.Any() ) {
							NoiseLogger.Current.LogMessage( string.Format( "Deleting Artist: {0}", artist.Name ));
							mSummary.ArtistsRemoved++;

							mArtistProvider.DeleteArtist( artist );
							mEventAggregator.Publish( new Events.ArtistRemoved( artist.DbId ));
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
							using( var contentList = mContentProvider.GetAlbumContentList( albumId )) {
								if( !contentList.List.Any() ) {
									if(!retValue.Contains( album.Artist )) {
										retValue.Add( album.Artist );
									}

									NoiseLogger.Current.LogMessage( string.Format( "Deleting Album: {0}", album.Name ));
									mSummary.AlbumsRemoved++;

									mAlbumProvider.DeleteAlbum( album );
									mEventAggregator.Publish( new Events.AlbumRemoved( album.DbId ));
								}
							}
						}
					}

					mArtistProvider.UpdateArtistLastChanged( album.Artist );
				}
			}

			return( retValue );
		}
	}
}
