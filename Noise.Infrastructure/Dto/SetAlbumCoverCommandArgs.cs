
namespace Noise.Infrastructure.Dto {
	public class SetAlbumCoverCommandArgs {
		public long		AlbumId { get; private set; }
		public long		ArtworkId { get; private set; }

		public SetAlbumCoverCommandArgs( long albumId, long artworkId ) {
			AlbumId = albumId;
			ArtworkId = artworkId;
		}
	}
}
