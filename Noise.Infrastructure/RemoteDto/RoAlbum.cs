using System;

namespace Noise.Infrastructure.RemoteDto {
	public class RoAlbum : RoBase {
		public string			Name { get; set; }
		public Int16			TrackCount { get; set; }
		public Int16			Rating { get; set; }
		public UInt32			PublishedYear { get; set; }
		public long				Genre { get; set; }
		public bool				IsFavorite { get; set; }
	}
}
