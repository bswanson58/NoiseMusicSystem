using System;
using System.IO;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Sidecars {
	internal class SidecarWriter {
		private readonly IFileWriter			mWriter;
		private readonly INoiseLog				mLog;
		private readonly IStorageFolderSupport	mStorageSupport;

		public SidecarWriter( IFileWriter writer, IStorageFolderSupport storageFolderSupport, INoiseLog log ) {
			mLog = log;
			mWriter = writer;
			mStorageSupport = storageFolderSupport;
		}

		public bool IsStorageAvailable( DbAlbum album ) {
			return( Directory.Exists( mStorageSupport.GetAlbumPath( album.DbId )));
		} 

		public void WriteSidecar( DbAlbum forAlbum, AlbumSidecar sidecar ) {
			var sidecarPath = Path.Combine( mStorageSupport.GetAlbumPath( forAlbum.DbId ), Constants.AlbumSidecarName );

			try {
				mWriter.Write( sidecarPath, sidecar );
			}
			catch( Exception exception ) {
				mLog.LogException( string.Format( "Writing sidecar to \"{0}\"", sidecarPath ), exception );
			}
		}

		public AlbumSidecar ReadSidecar( DbAlbum forAlbum ) {
			var retValue = default( AlbumSidecar );
			var sidecarPath = Path.Combine( mStorageSupport.GetAlbumPath( forAlbum.DbId ), Constants.AlbumSidecarName );

			try {
				if( File.Exists( sidecarPath )) {
					retValue = mWriter.Read<AlbumSidecar>( sidecarPath );
				}
			}
			catch( Exception ex ) {
				mLog.LogException( string.Format( "Reading sidecar from \"{0}\"", sidecarPath ), ex );
			}

			if( Equals( retValue, default( AlbumSidecar ))) {
				retValue = new AlbumSidecar( forAlbum );
			}

			return( retValue );
		}
	}
}
