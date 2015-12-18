using System;
using System.IO;
using System.Linq;
using Noise.Core.Logging;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Sidecars {
	internal class SidecarWriter : ISidecarWriter, ISidecarUpdater {
		private readonly IFileWriter					mWriter;
		private readonly ILogLibraryBuildingSidecars	mLog;
		private readonly ISidecarProvider				mSidecarProvider;
		private readonly ISidecarCreator				mSidecarCreator;
		private readonly IStorageFileProvider			mStorageFileProvider;
		private readonly IStorageFolderSupport			mStorageSupport;

		public SidecarWriter( IFileWriter writer, ISidecarProvider sidecarProvider, ISidecarCreator sidecarCreator,
							  IStorageFolderSupport storageFolderSupport, IStorageFileProvider storageFileProvider, ILogLibraryBuildingSidecars log ) {
			mLog = log;
			mWriter = writer;
			mSidecarProvider = sidecarProvider;
			mSidecarCreator = sidecarCreator;
			mStorageSupport = storageFolderSupport;
			mStorageFileProvider = storageFileProvider;
		}

		public bool IsStorageAvailable( DbAlbum album ) {
			return( IsStorageAvailable( album.DbId ));
		}

		private bool IsStorageAvailable( long albumId ) {
			return( Directory.Exists( mStorageSupport.GetAlbumPath( albumId )));
		}

		public bool IsStorageAvailable( DbArtist artist ) {
			return( Directory.Exists( mStorageSupport.GetArtistPath( artist.DbId )));
		}

		public void UpdateSidecar( DbTrack forTrack ) {
			var sidecar = new ScTrack( forTrack );

			WriteSidecar( forTrack, sidecar );
		}

		public void WriteSidecar( DbTrack forTrack, ScTrack sidecar ) {
			if( IsStorageAvailable( forTrack.Album )) {
				var albumPath = mStorageSupport.GetAlbumPath( forTrack.Album );
				var sidecarPath = Path.Combine( albumPath, Constants.AlbumSidecarName );

				try {
					var scAlbum = File.Exists( sidecarPath ) ? mWriter.Read<ScAlbum>( sidecarPath ) : 
															   mSidecarCreator.CreateFrom( forTrack );
					var scTrack = LocateTrackSidecar( forTrack, scAlbum );

					if( scTrack != null ) {
						scAlbum.TrackList.Remove( scTrack );
					}
					scAlbum.TrackList.Add( sidecar );

					mWriter.Write( sidecarPath, scAlbum );

					mLog.LogWriteSidecar( scAlbum );
				}
				catch( Exception exception ) {
					mLog.LogException( String.Format( "Writing {0} to \"{1}\"", sidecar, sidecarPath ), exception );
				}

				InsureSideCarStorageFileExists( forTrack.Album, sidecarPath, "(Track)" + forTrack.Name );
			}
		}

		private ScTrack LocateTrackSidecar( DbTrack forTrack, ScAlbum scAlbum ) {
			return( scAlbum.TrackList.FirstOrDefault( scTrack => forTrack.Name.Equals( scTrack.TrackName, StringComparison.CurrentCultureIgnoreCase ) &&
																 forTrack.TrackNumber == scTrack.TrackNumber &&
																 forTrack.VolumeName.Equals( scTrack.VolumeName, StringComparison.CurrentCultureIgnoreCase )));
		}

		public void UpdateSidecar( DbAlbum forAlbum ) {
			if( forAlbum != null ) {
				WriteSidecar( forAlbum, mSidecarCreator.CreateFrom( forAlbum ));
			}
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
					mLog.LogException( String.Format( "Writing {0} to \"{1}\"", sidecar, sidecarPath ), exception );
				}

				InsureSideCarStorageFileExists( forAlbum, sidecarPath );
			}
		}

		public void UpdateSidecar( DbArtist forArtist ) {
			var sidecar = new ScArtist( forArtist );

			WriteSidecar( forArtist, sidecar );
		}

		public void WriteSidecar( DbArtist forArtist, ScArtist sidecar ) {
			if( IsStorageAvailable( forArtist )) {
				var sidecarPath = Path.Combine( mStorageSupport.GetArtistPath( forArtist.DbId ), Constants.ArtistSidecarName );

				try {
					mWriter.Write( sidecarPath, sidecar );

					mLog.LogWriteSidecar( sidecar );
				}
				catch( Exception exception ) {
					mLog.LogException( String.Format( "Writing {0} to \"{1}\"", sidecar, sidecarPath ), exception );
				}

				InsureSideCarStorageFileExists( forArtist, sidecarPath );
			}
		}

		private void InsureSideCarStorageFileExists( DbArtist artist, string sidecarPath ) {
			try {
				if( File.Exists( sidecarPath )) {
					var storageFile = mStorageFileProvider.GetFileForMetadata( artist.DbId );

					if( storageFile == null ) {
						var file = new FileInfo( sidecarPath );
						var folder = mStorageSupport.GetArtistFolder( artist.DbId );

						mStorageFileProvider.AddFile( new StorageFile( file.Name, folder.DbId, file.Length, file.LastWriteTime )
																		{ FileType = eFileType.Sidecar, MetaDataPointer = artist.DbId });
					}
				}
				else {
					mLog.LogException( String.Format( "InsureSideCarStorageFileExists called with non-existent sidecar: '{0}'", sidecarPath ),
										new FileNotFoundException( "", sidecarPath ));
				}
			}
			catch( Exception exception ) {
				mLog.LogException( String.Format( "Creating SideCar StorageFile for {0}", artist ), exception );
			}
		}

		private void InsureSideCarStorageFileExists( DbAlbum album, string sidecarPath ) {
			InsureSideCarStorageFileExists( album.DbId, sidecarPath, album.Name );
		}

		private void InsureSideCarStorageFileExists( long albumId, string sidecarPath, string albumName ) {
			try {
				var storageFile = mStorageFileProvider.GetFileForMetadata( albumId );

				if( storageFile == null ) {
					var file = new FileInfo( sidecarPath );
					var folder = mStorageSupport.GetAlbumFolder( albumId );

					mStorageFileProvider.AddFile( new StorageFile( file.Name, folder.DbId, file.Length, file.LastWriteTime ) 
																	{ FileType = eFileType.Sidecar, MetaDataPointer = albumId });
				}
			}
			catch( Exception exception ) {
				mLog.LogException( String.Format( "Creating SideCar StorageFile for {0}", albumName ), exception );
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
				mLog.LogException( String.Format( "Reading sidecar from \"{0}\"", sidecarPath ), ex );
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
				mLog.LogException( String.Format( "Reading sidecar from \"{0}\"", sidecarPath ), ex );
			}

			if( Equals( retValue, default( ScAlbum ))) {
				retValue = new ScAlbum( forAlbum );
			}

			return( retValue );
		}

		public ScTrack ReadSidecar( DbTrack forTrack ) {
			var retValue = default( ScTrack );
			var sidecarPath = Path.Combine( mStorageSupport.GetAlbumPath( forTrack.Album ), Constants.AlbumSidecarName );

			try {
				if( File.Exists( sidecarPath )) {
					var scAlbum = mWriter.Read<ScAlbum>( sidecarPath );

					retValue = LocateTrackSidecar( forTrack, scAlbum );
				}
			}
			catch( Exception ex ) {
				mLog.LogException( String.Format( "Reading sidecar from \"{0}\"", sidecarPath ), ex );
			}

			if( Equals( retValue, default( ScTrack ))) {
				retValue = new ScTrack( forTrack );
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
