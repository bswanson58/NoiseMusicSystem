using System;

namespace Noise.Infrastructure.Dto {
	public class AlbumSidecar {
		public bool		IsFavorite { get; set; }
		public Int16	Rating { get; set; }
		public Int32	PublishedYear { get; set; }
		public long		Version { get; set; }

		public AlbumSidecar() { } // For Json deserialization.

		public AlbumSidecar( DbAlbum album ) {
			IsFavorite = album.IsFavorite;
			Rating = album.Rating;
			PublishedYear = album.PublishedYear;
			Version = album.Version;
		}
	}
}
