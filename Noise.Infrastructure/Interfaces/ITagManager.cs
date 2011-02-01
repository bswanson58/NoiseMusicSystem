using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface ITagManager {
		bool		Initialize();

		long		ResolveGenre( string genreName );
		DbGenre		GetGenre( long genreId );

		IEnumerable<DbDecadeTag>	DecadeTagList { get; }
		IEnumerable<long>			ArtistList( DbDecadeTag forDecade );
	}
}
