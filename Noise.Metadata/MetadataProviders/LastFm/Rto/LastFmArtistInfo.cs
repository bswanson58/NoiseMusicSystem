using System.Collections.Generic;
using Newtonsoft.Json;

namespace Noise.Metadata.MetadataProviders.LastFm.Rto {
	public class LastFmArtistInfoResult {
		public LastFmArtistInfo			Artist { get; set; }
	}

	public class LastFmArtistInfo {
		public string					Name { get; set; }
		public string					MbId { get; set; }
		public string					Url { get; set; }
		public int						OnTour { get; set; }

		[JsonProperty( "image" )]
		public List<LastFmImage>		ImageList { get; set; }

		[JsonConverter( typeof( SingleOrObjectConverter<LastFmSimilarArtistList> ))]
		public LastFmSimilarArtistList	Similar { get; set; }

		[JsonConverter( typeof( SingleOrObjectConverter<LastFmTagList> ))]
		public LastFmTagList			Tags { get; set; }

		public LastFmBio				Bio { get; set; }

		public LastFmArtistInfo() {
			ImageList = new List<LastFmImage>();
		}
	}

	public class LastFmSimilarArtistList {
		[JsonProperty( "artist" )]
		[JsonConverter( typeof( SingleOrArrayConverter<LastFmSimilarArtist> ))]
		public List<LastFmSimilarArtist> ArtistList { get; set; }

		public LastFmSimilarArtistList() {
			ArtistList = new List<LastFmSimilarArtist>();
		}
	}

	public class LastFmSimilarArtist {
		public string					Name { get; set; }
		public string					Url { get; set; }
		[JsonProperty( "image" )]
		public List<LastFmImage>		ImageList { get; set; }

		public LastFmSimilarArtist() {
			ImageList = new List<LastFmImage>();
		}
	}

	public class LastFmTagList {
		[JsonProperty( "tag" )]
		[JsonConverter( typeof( SingleOrArrayConverter<LastFmTag> ))]
		public List<LastFmTag>			TagList {get; set; }

		public LastFmTagList() {
			TagList = new List<LastFmTag>();
		}
	}

	public class LastFmTag {
		public string					Name {get; set; }
		public string					Url {get; set; }
	}

	public class LastFmBio {
		public string					Published { get; set; }
		public string					Summary { get; set; }
		public string					Content { get; set; }
		public int						YearFormed { get; set; }
	}

}
