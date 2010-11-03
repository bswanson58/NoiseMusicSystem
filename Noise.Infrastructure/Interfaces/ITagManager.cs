using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface ITagManager {
		long		ResolveGenre( string genreName );
		DbGenre		GetGenre( long genreId );
	}
}
