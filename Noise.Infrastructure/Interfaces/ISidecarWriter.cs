using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface ISidecarWriter {
		ScAlbum ReadSidecar( DbAlbum forAlbum );
		ScTrack	ReadSidecar( DbTrack forTrack );

		void	WriteSidecar( DbAlbum forAlbum, ScAlbum sidecar );
		void	WriteSidecar( DbTrack forTrack, ScTrack sidecar );
	}
}
