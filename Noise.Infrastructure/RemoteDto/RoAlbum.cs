using System;
using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class RoAlbum : RoBase {
		[DataMember]
		public string			Name { get; set; }
		[DataMember]
		public long				ArtistId { get; set; }
		[DataMember]
		public Int16			TrackCount { get; set; }
		[DataMember]
		public Int16			Rating { get; set; }
		[DataMember]
		public UInt32			PublishedYear { get; set; }
		[DataMember]
		public long				Genre { get; set; }
		[DataMember]
		public bool				IsFavorite { get; set; }
	}
}
