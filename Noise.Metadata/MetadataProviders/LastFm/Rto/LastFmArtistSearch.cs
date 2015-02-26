using System.Collections.Generic;
using Newtonsoft.Json;

namespace Noise.Metadata.MetadataProviders.LastFm.Rto {
	public class LastFmAristSearch {
		[JsonProperty( Required = Required.Default )]
		public LastFmResults			Results { get; set; }
	}

	public class LastFmResults {
		[JsonConverter( typeof( SingleOrObjectConverter<LastFmArtistList> ))]
		public LastFmArtistList			ArtistMatches { get; set; }

		[JsonProperty( "opensearch:totalResults" )]
		public int						TotalResults { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public class LastFmArtistList {
		public int						TotalResults { get; set; }

		[JsonProperty( "artist", Required = Required.Default )]
		[JsonConverter( typeof( SingleOrArrayConverter<LastFmArtist> ))]
		public List<LastFmArtist>		ArtistList { get; set; }

		public LastFmArtistList() {
			ArtistList = new List<LastFmArtist>();
		}
	}

	public class LastFmArtist {
		public string					Name { get; set; }
		public int						Listeners { get; set; }
		public string					MbId { get; set; }
		public string					Url { get; set; }
		[JsonProperty( "image" )]
		public List<LastFmImage>		ImageList { get; set; }

		public LastFmArtist() {
			ImageList = new List<LastFmImage>();
		}
	}

	public class LastFmImage {
		[JsonProperty("#text")]
		public string					Url { get; set; }
		public string					Size { get; set; }
	}
}
