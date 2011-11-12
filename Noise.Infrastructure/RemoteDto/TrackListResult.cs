using Noise.Infrastructure.RemoteHost;

namespace Noise.Infrastructure.RemoteDto {
	public class TrackListResult : BaseResult {
		public	RoTrack[]	Tracks { get; set; }

		public TrackListResult() {
			Tracks = new RoTrack[0];
		}
	}
}
