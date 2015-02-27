using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Noise.Metadata.MetadataProviders.LastFm.Rto {
	public class LastFmTopTracks {
		[JsonProperty( Required = Required.Default )]
		public LastFmTrackList		TopTracks { get; set; }

		[JsonProperty( Required = Required.Default )]
		public int					Error { get; set; }
		[JsonProperty( Required = Required.Default )]
		public string				Message { get; set; }
	}

	public class LastFmTrackList {
		[JsonProperty( "track" )]
		public List<LastFmTrack>	TrackList { get; set; }

		public LastFmTrackList() {
			TrackList = new List<LastFmTrack>();
		}
	}

	[DebuggerDisplay("Track = {Name}")]
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
