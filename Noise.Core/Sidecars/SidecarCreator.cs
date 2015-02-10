using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Sidecars {
	internal class SidecarCreator {
		private readonly IAlbumProvider	mAlbumProvider;

		public SidecarCreator( IAlbumProvider albumProvider ) {
			mAlbumProvider = albumProvider;
		}

		public AlbumSidecar CreateFromAlbum( DbAlbum album ) {
			return( new AlbumSidecar( album ));
		}

		public void UpdateAlbum( DbAlbum album, AlbumSidecar sidecar ) {
			using( var updater = mAlbumProvider.GetAlbumForUpdate( album.DbId ) ) {
				if( updater.Item != null ) {
					updater.Item.IsFavorite = sidecar.IsFavorite;
					updater.Item.Rating = sidecar.Rating;
					updater.Item.PublishedYear = sidecar.PublishedYear;
					updater.Item.SetVersionPreUpdate( sidecar.Version );
					
					album.IsFavorite = sidecar.IsFavorite;
					album.Rating = sidecar.Rating;
					album.PublishedYear = sidecar.PublishedYear;
					album.Version = sidecar.Version;

					updater.Update();
				}
			}
		}
	}
}
