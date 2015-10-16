using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Noise.Metadata.MetadataProviders.LastFm.Rto {
	public class LastFmAristSearch {
		[JsonProperty( Required = Required.Default )]
		public LastFmResults			Results { get; set; }

		[JsonProperty( Required = Required.Default )]
		public int						Error { get; set; }
		[JsonProperty( Required = Required.Default )]
		public string					Message { get; set; }
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

	[DebuggerDisplay("Artist = {Name}")]
	public class LastFmArtist {
		public string					Name { get; set; }

		[JsonProperty( NullValueHandling = NullValueHandling.Ignore )]
		public int						Listeners { get; set; }
	
		public string					MbId { get; set; }
		public string					Url { get; set; }
		[JsonProperty( "image" )]
		public List<LastFmImage>		ImageList { get; set; }

		public LastFmArtist() {
			ImageList = new List<LastFmImage>();
		}
	}

	[DebuggerDisplay("Image (Size = {Size})")]
	public class LastFmImage {
		[JsonProperty("#text")]
		public string					Url { get; set; }
		public string					Size { get; set; }
	}
}
