using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface ISidecarUpdater {
		void	UpdateSidecar( DbArtist forArtist );
		void	UpdateSidecar( DbAlbum forAlbum );
		void	UpdateSidecar( DbTrack forTrack );
	}
}
