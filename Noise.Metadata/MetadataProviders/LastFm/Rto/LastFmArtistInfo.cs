using System.Collections.Generic;

namespace Noise.Metadata.MetadataProviders.LastFm.Rto {
	public class LastFmArtistInfoResult {
		public LastFmArtistInfo			Artist { get; set; }
	}

	public class LastFmArtistInfo {
		public string					Name { get; set; }
		public string					MbId { get; set; }
		public string					Url { get; set; }
		public int						OnTour { get; set; }
		public List<LastFmImage>		Image { get; set; }
		public LastFmSimilarArtistList	Similar { get; set; }
		public LastFmTagList			Tags { get; set; }
		public LastFmBio				Bio { get; set; }
	}

	public class LastFmSimilarArtistList {
		public List<LastFmSimilarArtist> Artist { get; set; }
	}

	public class LastFmSimilarArtist {
		public string					Name { get; set; }
		public string					Url { get; set; }
		public LastFmImage[]			Image { get; set; }
	}

	public class LastFmTagList {
		public List<LastFmTag>			Tag {get; set; } 
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
