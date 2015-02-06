using Noise.Infrastructure.Dto;

namespace Noise.Core.Sidecars {
	internal class SidecarCreator {

		public AlbumSidecar CreateFromAlbum( DbAlbum album ) {
			return( new AlbumSidecar( album ));
		}

		public void UpdateAlbum( DbAlbum album, AlbumSidecar sidecar ) {
			album.IsFavorite = sidecar.IsFavorite;
			album.Rating = sidecar.Rating;
			album.PublishedYear = sidecar.PublishedYear;
			album.Version = sidecar.Version;
		}
	}
}
