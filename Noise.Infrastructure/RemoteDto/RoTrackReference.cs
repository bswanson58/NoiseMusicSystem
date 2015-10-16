using System;
using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {

	[DataContract]
	public class RoTrackReference {
		[DataMember]
		public long		TrackId { get; set; }
		[DataMember]
		public long		AlbumId { get; set; }
		[DataMember]
		public long		Duration { get; set; }
		[DataMember]
		public Int16	TrackNumber { get; set; }
		[DataMember]
		public string	VolumeName { get; set; }
	}
}
