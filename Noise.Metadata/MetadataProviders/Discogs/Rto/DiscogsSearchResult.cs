using System.Collections.Generic;

namespace Noise.Metadata.MetadataProviders.Discogs.Rto {
	public class DiscogsSearchResult {
		public List<ArtistSearchResult>	Results { get; set; }

		public DiscogsSearchResult() {
			Results = new List<ArtistSearchResult>();
		}
	}

	public class ArtistSearchResult {
		public string	Thumb { get; set; }
		public string	Title { get; set; }
		public string	Type { get; set; }
		public string	Id { get; set; }
	}
}
