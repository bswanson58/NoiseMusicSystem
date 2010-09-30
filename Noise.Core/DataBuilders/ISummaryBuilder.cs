using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Core.DataBuilders {
	public interface ISummaryBuilder {
		void BuildSummaryData( IEnumerable<DbArtist> artistList );

		void Stop();
	}
}
