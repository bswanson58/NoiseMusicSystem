using System;
using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class RoArtist : RoBase {
		[DataMember]
		public string			Name { get; set; }
		[DataMember]
		public Int16			AlbumCount { get; set; }
		[DataMember]
		public Int16			Rating { get; set; }
		[DataMember]
		public long				Genre { get; set; }
		[DataMember]
		public bool				IsFavorite { get; set; }
	}
}
