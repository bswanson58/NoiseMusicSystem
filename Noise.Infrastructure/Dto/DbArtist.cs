using System;

namespace Noise.Infrastructure.Dto {
	public class DbArtist {
		public string			Name { get; set; }
		public string			Genre { get; set; }
		public Int16			Rating { get; set; }
		public Int16			AlbumCount { get; set; }
	}
}
