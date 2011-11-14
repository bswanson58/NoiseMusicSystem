using Noise.Infrastructure.RemoteHost;

namespace Noise.Infrastructure.RemoteDto {
	public class ArtistListResult : BaseResult {
		public	RoArtist[]		Artists { get; set; }

		public ArtistListResult() {
			Artists = new RoArtist[0];
		}
	}
}
