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

		public ScPlayContext GetAlbumContext( DbTrack forTrack ) {
			var retValue = new ScPlayContext();
			var album = mAlbumProvider.GetAlbum( forTrack.Album );

			if( album != null ) {
				var scAlbum = mSidecarWriter.ReadSidecar( album );

				retValue = scAlbum.PlaybackContext;
			}

			return( retValue );
		}

		public ScPlayContext GetTrackContext( DbTrack forTrack ) {
			var retValue = new ScPlayContext();
			var scTrack = mSidecarWriter.ReadSidecar( forTrack );

			if( scTrack != null ) {
				retValue = scTrack.PlaybackContext;
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
