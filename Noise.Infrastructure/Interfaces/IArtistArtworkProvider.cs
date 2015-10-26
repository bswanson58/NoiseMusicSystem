using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IArtistArtworkProvider {
		Artwork		GetRandomArtwork( DbArtist forArtist );
		int			ImageCount( DbArtist forArtist );
	}
}
