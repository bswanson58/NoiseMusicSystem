
namespace Noise.Infrastructure.RemoteDto {
	public class AlbumListResult : BaseResult {
		public long			ArtistId { get; set; }
		public RoAlbum[]	Albums { get; set; }

		public AlbumListResult() {
			Albums = new RoAlbum[0];
		}
	}
}
