using System;

namespace Noise.Infrastructure.Dto {
	public class DbAlbum {
		public string			Name { get; set; }
		public long				Artist { get; set; }
		public Int16			Rating { get; set; }
		public Int16			TrackCount { get; set; }
	}
}
