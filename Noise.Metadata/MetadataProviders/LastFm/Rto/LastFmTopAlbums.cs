using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Noise.Metadata.MetadataProviders.LastFm.Rto {
	public class LastFmTopAlbums {
		public LastFmAlbumList		TopAlbums { get; set; }
	}

	public class LastFmAlbumList {
		[JsonProperty( "album" )]
		public List<LastFmAlbum>	AlbumList { get; set; }

		public LastFmAlbumList() {
			AlbumList = new List<LastFmAlbum>();
		}
	}

	[DebuggerDisplay("Album = {Name}")]
	public class LastFmAlbum {
		public string				Name { get; set; }
		public int					PlayCount { get; set; }
		public string				MbId { get; set; }
		public string				Url { get; set; }

		[JsonProperty( "image" )]
		public List<LastFmImage>	ImageList { get; set; }

		public LastFmAlbum() {
			ImageList = new List<LastFmImage>();
		}
	}
}
