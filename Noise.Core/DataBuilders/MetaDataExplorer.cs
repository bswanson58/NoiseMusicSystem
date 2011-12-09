using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CuttingEdge.Conditions;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Core.DataProviders;
using Noise.Core.FileStore;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataBuilders {
	public class MetaDataExplorer : IMetaDataExplorer {
		private readonly IUnityContainer		mContainer;
		private readonly IDatabaseManager		mDatabaseManager;
		private readonly FileTagProvider		mTagProvider;
		private readonly FileNameProvider		mFileNameProvider;
		private readonly FolderStrategyProvider	mStrategyProvider;
		private readonly DefaultProvider		mDefaultProvider;
		private DatabaseCache<DbArtist>			mArtistCache;
		private DatabaseCache<DbAlbum>			mAlbumCache;
		private bool							mStopExploring;
		private DatabaseChangeSummary			mSummary;

		public  MetaDataExplorer( IUnityContainer container ) {
			mContainer = container;
			mDatabaseManager = mContainer.Resolve<IDatabaseManager>();

			mTagProvider = new FileTagProvider( mContainer );
			mFileNameProvider = new FileNameProvider( mContainer );
			mStrategyProvider = new FolderStrategyProvider( mContainer );
			mDefaultProvider = new DefaultProvider();
		}

		public void BuildMetaData( DatabaseChangeSummary summary ) {
			Condition.Requires( summary ).IsNotNull();
			mStopExploring = false;
			mSummary = summary;

			var database = mDatabaseManager.ReserveDatabase();
			try {
				var		fileEnum = from StorageFile file in database.Database where file.FileType == eFileType.Undetermined orderby file.ParentFolder select file;

				mArtistCache = new DatabaseCache<DbArtist>( from DbArtist artist in database.Database select artist );
				mAlbumCache = new DatabaseCache<DbAlbum>( from DbAlbum album in database.Database select album );

				foreach( var file in fileEnum ) {
					file.FileType = StorageHelpers.DetermineFileType( file );

					switch( file.FileType ) {
						case eFileType.Music:
							BuildMusicMetaData( database, file );
							break;

						case eFileType.Picture:
							BuildArtworkMetaData( database, file );
							break;

						case eFileType.Text:
							BuildInfoMetaData( database, file );
							break;

						case eFileType.Unknown:
							// Nothing that we are interested in.
							database.Database.Store( file );
							break;
					}

					if( mStopExploring ) {
						break;
					}
				}

				mArtistCache.Clear();
				mAlbumCache.Clear();
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Building Metadata:", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}
		}

		public void Stop() {
			mStopExploring = true;
		}

		private void BuildMusicMetaData( IDatabase database, StorageFile file ) {
			Condition.Requires( file ).IsNotNull();

			try {
				var	track = new DbTrack();
				var folderStrategy = StorageHelpers.GetFolderStrategy( database.Database, file );
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

				var artist = DetermineArtist( database, dataProviders );
				if( artist != null ) {
					var album = DetermineAlbum( database, dataProviders, artist );

					if( album != null ) {
						track.Name = DetermineTrackName( dataProviders );

						if(!string.IsNullOrWhiteSpace( track.Name )) {
							track.VolumeName = DetermineVolumeName( dataProviders );

							foreach( var provider in dataProviders ) {
								provider.AddAvailableMetaData( artist, album, track );
							}

							track.Album = album.DbId;
							database.Insert( track );

							file.MetaDataPointer = track.DbId;
							database.Store( file );

							mSummary.TracksAdded++;
							artist.UpdateLastChange();
							database.Store( artist );
						}
						else {
							NoiseLogger.Current.LogMessage( "Track name cannot be determined for file: {0}", StorageHelpers.GetPath( database.Database, file ));
						}
					}
					else {
						NoiseLogger.Current.LogMessage( "Album cannot be determined for file: {0}", StorageHelpers.GetPath( database.Database, file ));
					}
				}
				else {
					NoiseLogger.Current.LogMessage( "Artist cannot determined for file: {0}", StorageHelpers.GetPath( database.Database, file ));
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( String.Format( "Building Music Metadata for: {0}", StorageHelpers.GetPath( database.Database, file )), ex );
			}
		}

		private void BuildInfoMetaData( IDatabase database, StorageFile file ) {
			Condition.Requires( file ).IsNotNull();

			try {
				var	info = new DbTextInfo( file.DbId, ContentType.TextInfo )
											{ Source = InfoSource.File, FolderLocation = file.ParentFolder, IsContentAvailable = true,
											  Name = Path.GetFileNameWithoutExtension( file.Name ) };
				var dataProviders = new List<IMetaDataProvider> { mStrategyProvider.GetProvider( file ),
																  mDefaultProvider.GetProvider( file ) };

				var artist = DetermineArtist( database, dataProviders );
				if( artist != null ) {
					info.Artist = artist.DbId;

					var album = DetermineAlbum( database, dataProviders, artist );

					if( album != null ) {
						info.Album = album.DbId;
					}

					artist.UpdateLastChange();
					database.Store( artist );
				}

				database.BlobStorage.Store( info.DbId, StorageHelpers.GetPath( database.Database, file ));
				database.Insert( info );

				file.MetaDataPointer = info.DbId;
				database.Store( file );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( String.Format( "Building Info Metadata for: {0}", StorageHelpers.GetPath( database.Database, file )), ex );
			}
		}

		private void BuildArtworkMetaData( IDatabase database, StorageFile file ) {
			Condition.Requires( file ).IsNotNull();

			try {
				var	artwork = new DbArtwork( file.DbId, StorageHelpers.IsCoverFile( file.Name ) ? ContentType.AlbumCover : ContentType.AlbumArtwork )
											{ Source = InfoSource.File, FolderLocation = file.ParentFolder, IsContentAvailable = true,
											  Name = Path.GetFileNameWithoutExtension( file.Name )};
				var dataProviders = new List<IMetaDataProvider> { mStrategyProvider.GetProvider( file ),
																  mDefaultProvider.GetProvider( file ) };

				var artist = DetermineArtist( database, dataProviders );
				if( artist != null ) {
					artwork.Artist = artist.DbId;

					var album = DetermineAlbum( database, dataProviders, artist );

					if( album != null ) {
						artwork.Album = album.DbId;
					}

					artist.UpdateLastChange();
					database.Store( artist );
				}

				database.BlobStorage.Insert( artwork.DbId, StorageHelpers.GetPath( database.Database, file ));
				database.Insert( artwork );

				file.MetaDataPointer = artwork.DbId;
				database.Store( file );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( String.Format( "Building Artwork Metadata for: {0}", StorageHelpers.GetPath( database.Database, file )), ex );
			}
		}

		private DbArtist DetermineArtist( IDatabase database, IEnumerable<IMetaDataProvider> providers ) {
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

					database.Insert( retValue );
					mArtistCache.Add( retValue );

					mSummary.ArtistsAdded++;
					NoiseLogger.Current.LogInfo( "Added artist: {0}", retValue.Name );
				}
			}

			return( retValue );
		}

		private DbAlbum DetermineAlbum( IDatabase database, IEnumerable<IMetaDataProvider> providers, DbArtist artist ) {
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

					database.Insert( retValue );
					mAlbumCache.Add( retValue );

					artist.AlbumCount++;
					database.Store( artist );

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
