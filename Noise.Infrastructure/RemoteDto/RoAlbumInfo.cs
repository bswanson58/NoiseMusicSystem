using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class RoAlbumInfo {
		[DataMember]
		public long			AlbumId { get; set; }
		[DataMember]
		public string		AlbumCover { get; set; }
	}
}
