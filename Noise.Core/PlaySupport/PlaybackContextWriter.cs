using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlaySupport {
	public class PlaybackContextWriter : IPlaybackContextWriter {
		private readonly IAlbumProvider		mAlbumProvider;
		private readonly ISidecarWriter		mSidecarWriter;

		public PlaybackContextWriter( IAlbumProvider albumProvider, ISidecarWriter sidecarWriter ) {
			mAlbumProvider = albumProvider;
			mSidecarWriter = sidecarWriter;
		}

		public PlaybackContext GetAlbumContext( DbTrack forTrack ) {
			var retValue = new PlaybackContext();
			var album = mAlbumProvider.GetAlbum( forTrack.Album );

			if( album != null ) {
				var scAlbum = mSidecarWriter.ReadSidecar( album );

				retValue.AddContext( scAlbum.PlaybackContext );
			}

			return( retValue );
		}

		public PlaybackContext GetTrackContext( DbTrack forTrack ) {
			var retValue = new PlaybackContext();
			var scTrack = mSidecarWriter.ReadSidecar( forTrack );

			if( scTrack != null ) {
				retValue.AddContext( scTrack.PlaybackContext );
			}

			return( retValue );
		}

		public void SaveAlbumContext( DbTrack forTrack, ScPlayContext albumContext ) {
			var album = mAlbumProvider.GetAlbum( forTrack.Album );

			if( album != null ) {
				var scAlbum = mSidecarWriter.ReadSidecar( album );

				if( scAlbum != null ) {
					scAlbum.PlaybackContext = albumContext;
					mSidecarWriter.WriteSidecar( album, scAlbum );
				}
			}			
		}

		public void SaveTrackContext( DbTrack forTrack, ScPlayContext trackContext ) {
			var scTrack = mSidecarWriter.ReadSidecar( forTrack );

			if( scTrack != null ) {
				scTrack.PlaybackContext = trackContext;
				mSidecarWriter.WriteSidecar( forTrack, scTrack );
			}
		}
	}
}
