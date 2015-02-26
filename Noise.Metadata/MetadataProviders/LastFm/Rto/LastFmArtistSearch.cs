using System.Collections.Generic;
using Newtonsoft.Json;

namespace Noise.Metadata.MetadataProviders.LastFm.Rto {
	public class LastFmAristSearch {
		[JsonProperty( Required = Required.Default )]
		public LastFmResults			Results { get; set; }
	}

	public class LastFmResults {
		public LastFmArtistList			ArtistMatches { get; set; }
	}

	public class LastFmArtistList {
		public List<LastFmArtist>		Artist { get; set; }
	}

	public class LastFmArtist {
		public string					Name { get; set; }
		public int						Listeners { get; set; }
		public string					MbId { get; set; }
		public string					Url { get; set; }
		public List<LastFmImage>		Image { get; set; }
	}

	public class LastFmImage {
		[JsonProperty("#text")]
		public string					Url { get; set; }
		public string					Size { get; set; }
	}
}
