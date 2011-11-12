using System;

namespace Noise.Infrastructure.RemoteDto {
	public class RoArtist : RoBase {
		public string			Name { get; set; }
		public string			Website { get; set; }
		public Int16			AlbumCount { get; set; }
		public Int16			Rating { get; set; }
		public long				Genre { get; set; }
		public bool				IsFavorite { get; set; }
	}
}
