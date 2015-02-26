using System.Collections.Generic;

namespace Noise.Metadata.MetadataProviders.LastFm.Rto {
	public class LastFmTopAlbums {
		public LastFmAlbumList		TopAlbums { get; set; }
	}

	public class LastFmAlbumList {
		public List<LastFmAlbum>	Album { get; set; }
	}

	public class LastFmAlbum {
		public string				Name { get; set; }
		public int					PlayCount { get; set; }
		public string				MbId { get; set; }
		public string				Url { get; set; }
		public List<LastFmImage>	Image { get; set; }
	}
}
