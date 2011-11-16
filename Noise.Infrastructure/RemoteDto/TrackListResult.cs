using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class TrackListResult : BaseResult {
		[DataMember]
		public long			ArtistId { get; set; }
		[DataMember]
		public long			AlbumId { get; set; }
		[DataMember]
		public	RoTrack[]	Tracks { get; set; }

		public TrackListResult() {
			Tracks = new RoTrack[0];
		}
	}
}
