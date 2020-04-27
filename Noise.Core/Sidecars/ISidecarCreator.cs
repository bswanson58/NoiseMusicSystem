using Noise.Infrastructure.Dto;

namespace Noise.Core.Sidecars {
	public interface ISidecarCreator {
		ScArtist	CreateFrom( DbArtist artist );
		ScAlbum		CreateFrom( DbAlbum album );
		ScAlbum		CreateFrom( DbTrack track );

        void		UpdateSidecar( ScAlbum sidecar, DbAlbum album );

		void		Update( DbArtist artist, ScArtist sidecar );
		void		Update( DbAlbum album, ScAlbum sidecar );
	}
}
