using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IArtworkProvider {
		Artwork						GetArtistArtwork( long artistId, ContentType ofType );
		Artwork[]					GetAlbumArtwork( long albumId, ContentType ofType );

		DataUpdateShell<DbArtwork>	GetArtworkForUpdate( long artworkId );
	}
}
