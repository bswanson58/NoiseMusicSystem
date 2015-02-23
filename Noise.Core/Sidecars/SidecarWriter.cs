using System;
using System.IO;
using Noise.Core.Logging;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Sidecars {
	internal class SidecarWriter {
		private readonly IFileWriter					mWriter;
		private readonly ILogLibraryBuildingSidecars	mLog;
		private readonly ISidecarProvider				mSidecarProvider;
		private readonly IStorageFileProvider			mStorageFileProvider;
		private readonly IStorageFolderSupport			mStorageSupport;

		public SidecarWriter( IFileWriter writer, ISidecarProvider sidecarProvider, IStorageFolderSupport storageFolderSupport, IStorageFileProvider storageFileProvider,
							  ILogLibraryBuildingSidecars log ) {
			mLog = log;
			mWriter = writer;
			mSidecarProvider = sidecarProvider;
			mStorageSupport = storageFolderSupport;
			mStorageFileProvider = storageFileProvider;
		}

		public bool IsStorageAvailable( DbAlbum album ) {
			return( Directory.Exists( mStorageSupport.GetAlbumPath( album.DbId )));
		} 

		public bool IsStorageAvailable( DbArtist artist ) {
			return( Directory.Exists( mStorageSupport.GetArtistPath( artist.DbId )));
		} 

		public void WriteSidecar( DbAlbum forAlbum, ScAlbum sidecar ) {
			if( IsStorageAvailable( forAlbum )) {
				var albumPath = mStorageSupport.GetAlbumPath( forAlbum.DbId );
				var sidecarPath = Path.Combine( albumPath, Constants.AlbumSidecarName );

				try {
					mWriter.Write( sidecarPath, sidecar );

					mLog.LogWriteSidecar( sidecar );
				}
				catch( Exception exception ) {
					mLog.LogException( string.Format( "Writing {0} to \"{1}\"", sidecar, sidecarPath ), exception );
				}

				InsureSideCarStorageFileExists( forAlbum, sidecarPath );
			}
		}

		public void WriteSidecar( DbArtist forArtist, ScArtist sidecar ) {
			if( IsStorageAvailable( forArtist )) {
				var sidecarPath = Path.Combine( mStorageSupport.GetArtistPath( forArtist.DbId ), Constants.ArtistSidecarName );

				try {
					mWriter.Write( sidecarPath, sidecar );

					mLog.LogWriteSidecar( sidecar );
				}
				catch( Exception exception ) {
					mLog.LogException( string.Format( "Writing {0} to \"{1}\"", sidecar, sidecarPath ), exception );
				}

				InsureSideCarStorageFileExists( forArtist, sidecarPath );
			}
		}

		private void InsureSideCarStorageFileExists( DbArtist artist, string sidecarPath ) {
			try {
				var storageFile = mStorageFileProvider.GetFileForMetadata( artist.DbId );

				if( storageFile == null ) {
					var file = new FileInfo( sidecarPath );
					var folder = mStorageSupport.GetArtistFolder( artist.DbId );

					mStorageFileProvider.AddFile( new StorageFile( file.Name, folder.DbId, file.Length, file.LastWriteTime )
																	{ FileType = eFileType.Sidecar, MetaDataPointer = artist.DbId });
				}
			}
			catch( Exception exception ) {
				mLog.LogException( string.Format( "Creating SideCar StorageFile for {0}", artist ), exception );
			}
		}

		private void InsureSideCarStorageFileExists( DbAlbum album, string sidecarPath ) {
			try {
				var storageFile = mStorageFileProvider.GetFileForMetadata( album.DbId );

				if( storageFile == null ) {
					var file = new FileInfo( sidecarPath );
					var folder = mStorageSupport.GetAlbumFolder( album.DbId );

					mStorageFileProvider.AddFile( new StorageFile( file.Name, folder.DbId, file.Length, file.LastWriteTime ) 
																	{ FileType = eFileType.Sidecar, MetaDataPointer = album.DbId });
				}
			}
			catch( Exception exception ) {
				mLog.LogException( string.Format( "Creating SideCar StorageFile for {0}", album ), exception );
			}
		}

		public ScArtist ReadSidecar( DbArtist artist ) {
			var retValue = default( ScArtist );
			var sidecarPath = Path.Combine( mStorageSupport.GetArtistPath( artist.DbId ), Constants.ArtistSidecarName );

			try {
				if( File.Exists( sidecarPath )) {
					retValue = mWriter.Read<ScArtist>( sidecarPath );
				}
			}
			catch( Exception ex ) {
				mLog.LogException( string.Format( "Reading sidecar from \"{0}\"", sidecarPath ), ex );
			}

			if( Equals( retValue, default( ScArtist ))) {
				retValue = new ScArtist( artist );
			}

			return( retValue );
		}

		public ScAlbum ReadSidecar( DbAlbum forAlbum ) {
			var retValue = default( ScAlbum );
			var sidecarPath = Path.Combine( mStorageSupport.GetAlbumPath( forAlbum.DbId ), Constants.AlbumSidecarName );

			try {
				if( File.Exists( sidecarPath )) {
					retValue = mWriter.Read<ScAlbum>( sidecarPath );
				}
			}
			catch( Exception ex ) {
				mLog.LogException( string.Format( "Reading sidecar from \"{0}\"", sidecarPath ), ex );
			}

			if( Equals( retValue, default( ScAlbum ))) {
				retValue = new ScAlbum( forAlbum );
			}

			return( retValue );
		}

		public void UpdateSidecarVersion( DbAlbum album, StorageSidecar sidecar ) {
			using( var updater = mSidecarProvider.GetSidecarForUpdate( sidecar.DbId )) {
				if( updater.Item != null ) {
					updater.Item.Version = album.Version;
					sidecar.Version = album.Version;

					updater.Update();

					mLog.LogUpdatedSidecar( sidecar, album );
				}
			}
		}

		public void UpdateSidecarVersion( DbArtist artist, StorageSidecar sidecar ) {
			using( var updater = mSidecarProvider.GetSidecarForUpdate( sidecar.DbId )) {
				if( updater.Item != null ) {
					updater.Item.Version = artist.Version;
					sidecar.Version = artist.Version;

					updater.Update();

					mLog.LogUpdatedSidecar( sidecar, artist );
				}
			}
		}
	}
}
