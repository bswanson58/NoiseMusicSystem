using System.Collections.Generic;

namespace Noise.Metadata.MetadataProviders.LastFm.Rto {
	public class LastFmTopTracks {
		public LastFmTrackList		TopTracks { get; set; }
	}

	public class LastFmTrackList {
		public List<LastFmTrack>	Track { get; set; } 
	}

	public class LastFmTrack {
		public string				Name { get; set; }
		public int					Duration { get; set; }
		public int					PlayCount { get; set; }
		public int					Listeners { get; set; }
		public string				MbId { get; set; }
		public string				Url { get; set; }
		public List<LastFmImage>	Image { get; set; }
	}
}
