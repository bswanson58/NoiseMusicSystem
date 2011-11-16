using System;
using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class RoTrack : RoBase {
		[DataMember]
		public string			Name { get; set; }
		[DataMember]
		public long				ArtistId { get; set; }
		[DataMember]
		public long				AlbumId { get; set; }
		[DataMember]
		public Int32			DurationMilliseconds { get; set; }
		[DataMember]
		public Int16			Rating { get; set; }
		[DataMember]
		public UInt16			TrackNumber { get; set; }
		[DataMember]
		public string			VolumeName { get; set; }
		[DataMember]
		public bool				IsFavorite { get; set; }
	}
}
