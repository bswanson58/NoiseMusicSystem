using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IAlbumArtworkProvider {
		Artwork GetAlbumCover( DbAlbum forAlbum );
		Artwork GetNextAlbumArtwork( DbAlbum forAlbum, int index );
		Artwork GetRandomAlbumArtwork( DbAlbum forAlbum );

		int		ImageCount( DbAlbum forAlbum );

		void	SetAlbumCover( long albumId, long artworkId );
	}
}
