using System.Collections.Generic;
using Newtonsoft.Json;

namespace Noise.Metadata.MetadataProviders.LastFm.Rto {
	public class LastFmTopTracks {
		public LastFmTrackList		TopTracks { get; set; }
	}

	public class LastFmTrackList {
		[JsonProperty( "track" )]
		public List<LastFmTrack>	TrackList { get; set; }

		public LastFmTrackList() {
			TrackList = new List<LastFmTrack>();
		}
	}

	public class LastFmTrack {
		public string				Name { get; set; }
		public int					Duration { get; set; }
		public int					PlayCount { get; set; }
		public int					Listeners { get; set; }
		public string				MbId { get; set; }
		public string				Url { get; set; }

		[JsonProperty( "image" )]
		public List<LastFmImage>	ImageList { get; set; }

		public LastFmTrack() {
			ImageList = new List<LastFmImage>();
		}
	}
}
