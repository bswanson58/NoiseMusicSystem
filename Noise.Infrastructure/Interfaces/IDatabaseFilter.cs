using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IDatabaseFilter {
		bool	IsEnabled { get; }

		bool	ArtistMatch( DbArtist artist );
	}
}
