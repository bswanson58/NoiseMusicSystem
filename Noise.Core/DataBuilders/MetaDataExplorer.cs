using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.Core.Database;
using Noise.Core.DataProviders;
using Noise.Core.FileStore;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataBuilders {
	public class MetaDataExplorer : IMetaDataExplorer {
		private readonly IArtistProvider		mArtistProvider;
		private readonly IAlbumProvider		mAlbumProvider;
		private readonly ITrackProvider			mTrackProvider;
		private readonly IArtworkProvider		mArtworkProvider;
		private readonly ITextInfoProvider		mTextInfoProvider;
		private readonly ITagManager			mTagManager;
		private readonly FileTagProvider		mTagProvider;
		private readonly FileNameProvider		mFileNameProvider;
		private readonly FolderStrategyProvider	mStrategyProvider;
		private readonly IStorageFolderProvider	mStorageFolderProvider;
		private readonly IStorageFileProvider	mStorageFileProvider;
		private readonly DefaultProvider		mDefaultProvider;
		private DatabaseCache<DbArtist>			mArtistCache;
		private DatabaseCache<DbAlbum>			mAlbumCache;
		private bool							mStopExploring;
		private DatabaseChangeSummary			mSummary;

		public  MetaDataExplorer( IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider,
								  IArtworkProvider artworkProvider, ITextInfoProvider textInfoProvider, ITagManager tagManager,
								  IStorageFolderProvider folderProvider, IStorageFileProvider fileProvider ) {
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mArtworkProvider = artworkProvider;
			mTextInfoProvider = textInfoProvider;
			mTagManager = tagManager;
			mStorageFolderProvider = folderProvider;
			mStorageFileProvider = fileProvider;

			mTagProvider = new FileTagProvider( mTagManager, mArtworkProvider, mStorageFileProvider );
			mFileNameProvider = new FileNameProvider( mStorageFileProvider );
			mStrategyProvider = new FolderStrategyProvider( mStorageFolderProvider, mTagManager );
			mDefaultProvider = new DefaultProvider();
		}

		public void BuildMetaData( DatabaseChangeSummary summary ) {
			Condition.Requires( summary ).IsNotNull();
			mStopExploring = false;
			mSummary = summary;

			try {
				using( var fileList = mStorageFileProvider.GetFilesOfType( eFileType.Undetermined )) {
					var fileEnum = from file in fileList.List orderby file.ParentFolder select file;

					using( var artistList = mArtistProvider.GetArtistList()) {
						mArtistCache = new DatabaseCache<DbArtist>( artistList.List );
					}
					using( var albumList = mAlbumProvider.GetAllAlbums()) {
						mAlbumCache = new DatabaseCache<DbAlbum>( albumList.List );
					}

					foreach( var file in fileEnum ) {
						file.FileType = StorageHelpers.DetermineFileType( file );

						switch( file.FileType ) {
							case eFileType.Music:
								BuildMusicMetaData( file );
								break;

							case eFileType.Picture:
								BuildArtworkMetaData( file );
								break;

							case eFileType.Text:
								BuildInfoMetaData( file );
								break;

							case eFileType.Unknown:
								// Nothing that we are interested in.
								using( var updater = mStorageFileProvider.GetFileForUpdate( file.DbId )) {
									if( updater.Item != null ) {
										updater.Item.FileType = file.FileType;

										updater.Update();
									}
								}
								break;
						}

						if( mStopExploring ) {
							break;
						}
					}
				}

				mArtistCache.Clear();
				mAlbumCache.Clear();
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Building Metadata:", ex );
			}
		}

		public void Stop() {
			mStopExploring = true;
		}

		private void BuildMusicMetaData( StorageFile file ) {
			Condition.Requires( file ).IsNotNull();

			try {
				var	track = new DbTrack();
				var folderStrategy = StorageHelpers.GetFolderStrategy( mStorageFolderProvider, file );
				var dataProviders = new List<IMetaDataProvider>();

				track.Encoding = StorageHelpers.DetermineAudioEncoding( file );

				if( folderStrategy.PreferFolderStrategy ) {
					dataProviders.Add( mStrategyProvider.GetProvider( file ));
					dataProviders.Add( mTagProvider.GetProvider( file, track.Encoding ));
					dataProviders.Add( mFileNameProvider.GetProvider( file ));
					dataProviders.Add( mDefaultProvider.GetProvider( file ));
				}
				else {
					dataProviders.Add( mTagProvider.GetProvider( file, track.Encoding ));
					dataProviders.Add( mStrategyProvider.GetProvider( file ));
					dataProviders.Add( mFileNameProvider.GetProvider( file ));
					dataProviders.Add( mDefaultProvider.GetProvider( file ));
				}

				var artist = DetermineArtist( dataProviders );
				if( artist != null ) {
					var album = DetermineAlbum( dataProviders, artist );

					if( album != null ) {
						track.Name = DetermineTrackName( dataProviders );

						if(!string.IsNullOrWhiteSpace( track.Name )) {
							track.VolumeName = DetermineVolumeName( dataProviders );

							foreach( var provider in dataProviders ) {
								provider.AddAvailableMetaData( artist, album, track );
							}

							track.Album = album.DbId;
							mTrackProvider.AddTrack( track );

							using( var updater = mStorageFileProvider.GetFileForUpdate( file.DbId )) {
								if( updater.Item != null ) {
									updater.Item.MetaDataPointer = track.DbId;

									updater.Update();
								}
							}

							mSummary.TracksAdded++;

							using( var updater = mArtistProvider.GetArtistForUpdate( artist.DbId )) {
								if( updater.Item != null ) {
									updater.Item.UpdateLastChange();

									updater.Update();
								}
							}
						}
						else {
							NoiseLogger.Current.LogMessage( "Track name cannot be determined for file: {0}", mStorageFileProvider.GetPhysicalFilePath( file ));
						}
					}
					else {
						NoiseLogger.Current.LogMessage( "Album cannot be determined for file: {0}", mStorageFileProvider.GetPhysicalFilePath( file ));
					}
				}
				else {
					NoiseLogger.Current.LogMessage( "Artist cannot determined for file: {0}", mStorageFileProvider.GetPhysicalFilePath( file ));
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( String.Format( "Building Music Metadata for: {0}", mStorageFileProvider.GetPhysicalFilePath( file )), ex );
			}
		}

		private void BuildInfoMetaData( StorageFile file ) {
			Condition.Requires( file ).IsNotNull();

			try {
				var	info = new DbTextInfo( file.DbId, ContentType.TextInfo )
											{ Source = InfoSource.File, FolderLocation = file.ParentFolder, IsContentAvailable = true,
											  Name = Path.GetFileNameWithoutExtension( file.Name ) };
				var dataProviders = new List<IMetaDataProvider> { mStrategyProvider.GetProvider( file ),
																  mDefaultProvider.GetProvider( file ) };

				var artist = DetermineArtist( dataProviders );
				if( artist != null ) {
					info.Artist = artist.DbId;

					var album = DetermineAlbum( dataProviders, artist );

					if( album != null ) {
						info.Album = album.DbId;
					}

					using( var updater = mArtistProvider.GetArtistForUpdate( artist.DbId )) {
						if( updater.Item != null ) {
							updater.Item.UpdateLastChange();

							updater.Update();
						}
					}
				}

				mTextInfoProvider.AddTextInfo( info, mStorageFileProvider.GetPhysicalFilePath( file ));

				using( var updater = mStorageFileProvider.GetFileForUpdate( file.DbId )) {
					if( updater.Item != null ) {
						updater.Item.MetaDataPointer = info.DbId;

						updater.Update();
					}
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( String.Format( "Building Info Metadata for: {0}", mStorageFileProvider.GetPhysicalFilePath( file )), ex );
			}
		}

		private void BuildArtworkMetaData( StorageFile file ) {
			Condition.Requires( file ).IsNotNull();

			try {
				var	artwork = new DbArtwork( file.DbId, StorageHelpers.IsCoverFile( file.Name ) ? ContentType.AlbumCover : ContentType.AlbumArtwork )
											{ Source = InfoSource.File, FolderLocation = file.ParentFolder, IsContentAvailable = true,
											  Name = Path.GetFileNameWithoutExtension( file.Name )};
				var dataProviders = new List<IMetaDataProvider> { mStrategyProvider.GetProvider( file ),
																  mDefaultProvider.GetProvider( file ) };

				var artist = DetermineArtist( dataProviders );
				if( artist != null ) {
					artwork.Artist = artist.DbId;

					var album = DetermineAlbum( dataProviders, artist );

					if( album != null ) {
						artwork.Album = album.DbId;
					}

					using( var updater = mArtistProvider.GetArtistForUpdate( artist.DbId )) {
						if( updater.Item != null ) {
							updater.Item.UpdateLastChange();

							updater.Update();
						}
					}
				}

				mArtworkProvider.AddArtwork( artwork, mStorageFileProvider.GetPhysicalFilePath( file ));

				using( var updater = mStorageFileProvider.GetFileForUpdate( file.DbId )) {
					if( updater.Item != null ) {
						updater.Item.MetaDataPointer = artwork.DbId;

						updater.Update();
					}
				}
				file.MetaDataPointer = artwork.DbId;
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( String.Format( "Building Artwork Metadata for: {0}", mStorageFileProvider.GetPhysicalFilePath( file )), ex );
			}
		}

		private DbArtist DetermineArtist( IEnumerable<IMetaDataProvider> providers ) {
			DbArtist	retValue = null;
			var			artistName = "";

			foreach( var provider in providers ) {
				artistName = provider.Artist;

				if(!string.IsNullOrWhiteSpace( artistName )) {
					break;
				}
			}

			if(!string.IsNullOrWhiteSpace( artistName )) {
				retValue = mArtistCache.Find( artist => artist.Name == artistName );
				if( retValue == null ) {
					retValue = new DbArtist { Name = artistName };

					mArtistProvider.AddArtist( retValue );
					mArtistCache.Add( retValue );

					mSummary.ArtistsAdded++;
					NoiseLogger.Current.LogInfo( "Added artist: {0}", retValue.Name );
				}
			}

			return( retValue );
		}

		private DbAlbum DetermineAlbum( IEnumerable<IMetaDataProvider> providers, DbArtist artist ) {
			DbAlbum		retValue = null;
			var			albumName = "";

			foreach( var provider in providers ) {
				albumName = provider.Album;

				if(!string.IsNullOrWhiteSpace( albumName )) {
					break;
				}
			}

			if(!string.IsNullOrWhiteSpace( albumName )) {
				retValue = mAlbumCache.Find( album => album.Name == albumName && album.Artist == artist.DbId );
				if( retValue == null ) {
					retValue = new DbAlbum { Name = albumName, Artist = artist.DbId };

					mAlbumProvider.AddAlbum( retValue );
					mAlbumCache.Add( retValue );

					using( var updater = mArtistProvider.GetArtistForUpdate( artist.DbId )) {
						if( updater.Item != null ) {
							updater.Item.AlbumCount++;

							updater.Update();
						}
					}
					artist.AlbumCount++;

					mSummary.AlbumsAdded++;
					NoiseLogger.Current.LogInfo( "Added album: {0}", retValue.Name );
				}
			}

			return( retValue );
		}

		private static string DetermineTrackName( IEnumerable<IMetaDataProvider> providers ) {
			var retValue = "";

			foreach( var provider in providers ) {
				retValue = provider.TrackName;

				if(!string.IsNullOrWhiteSpace( retValue )) {
					break;
				}
			}

			return( retValue );
		}

		private static string DetermineVolumeName( IEnumerable<IMetaDataProvider> providers ) {
			var retValue = "";

			foreach( var provider in providers ) {
				retValue = provider.VolumeName;

				if(!string.IsNullOrWhiteSpace( retValue )) {
					break;
				}
			}

			return( retValue );
		}
	}
}
