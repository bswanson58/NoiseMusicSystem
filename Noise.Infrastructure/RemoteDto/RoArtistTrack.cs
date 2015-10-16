using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class RoArtistTrack {
		[DataMember]
		public string				TrackName { get; set; }
		[DataMember]
		public RoTrackReference[]	Tracks { get; set; }
	}
}
