using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class RoArtistTrack {
		[DataMember]
		public long		TrackId { get; set; }
		[DataMember]
		public long		ArtistId { get; set; }
		[DataMember]
		public string	TrackName { get; set; }
		[DataMember]
		public long[]	Albums { get; set; }
	}
}
