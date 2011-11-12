using Noise.Infrastructure.RemoteHost;

namespace Noise.Infrastructure.RemoteDto {
	public class AlbumListResult : BaseResult {
		public RoAlbum[]	Albums { get; set; }

		public AlbumListResult() {
			Albums = new RoAlbum[0];
		}
	}
}
