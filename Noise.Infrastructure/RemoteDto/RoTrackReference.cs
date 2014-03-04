using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {

	[DataContract]
	public class RoTrackReference {
		[DataMember]
		public long		TrackId { get; set; }
		[DataMember]
		public long		AlbumId { get; set; }
	}
}
