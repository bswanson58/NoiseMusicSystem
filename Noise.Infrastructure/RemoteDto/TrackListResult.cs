
namespace Noise.Infrastructure.RemoteDto {
	public class TrackListResult : BaseResult {
		public long			ArtistId { get; set; }
		public long			AlbumId { get; set; }
		public	RoTrack[]	Tracks { get; set; }

		public TrackListResult() {
			Tracks = new RoTrack[0];
		}
	}
}
