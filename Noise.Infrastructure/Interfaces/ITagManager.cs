using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface ITagManager {
		long		ResolveGenre( string genreName );
		DbGenre		GetGenre( long genreId );

		IEnumerable<DbDecadeTag>	DecadeTagList { get; }
		IEnumerable<long>			ArtistListForDecade( long decadeId );
		IEnumerable<long>			AlbumListForDecade( long artistId, long decadeId );
	}
}
