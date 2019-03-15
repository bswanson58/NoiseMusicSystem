using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface ISidecarWriter {
		bool		IsStorageAvailable( DbArtist artist );
		bool		IsStorageAvailable( DbAlbum album );

		ScArtist	ReadSidecar( DbArtist artist );
		ScAlbum		ReadSidecar( DbAlbum forAlbum );
		ScTrack		ReadSidecar( DbTrack forTrack );

		void		WriteSidecar( DbArtist forArtist, ScArtist sidecar );
		void		WriteSidecar( DbAlbum forAlbum, ScAlbum sidecar );
		void		WriteSidecar( DbTrack forTrack, ScTrack sidecar );

		void		UpdateSidecarVersion( DbArtist artist, StorageSidecar sidecar );
		void		UpdateSidecarVersion( DbAlbum album, StorageSidecar sidecar );
	}
}
