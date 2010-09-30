using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Core.DataBuilders {
	public interface ISearchBuilder {
		void	BuildSearchIndex( IEnumerable<DbArtist> artistList );

		void	Stop();
	}
}
