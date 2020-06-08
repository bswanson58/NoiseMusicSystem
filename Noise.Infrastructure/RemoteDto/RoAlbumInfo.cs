using System.Runtime.Serialization;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class RoAlbumInfo {
		[DataMember]
		public long			AlbumId { get; set; }
		[DataMember]
		public string		AlbumCover { get; set; }

		public RoAlbumInfo( DbAlbum album ) {
			AlbumId = album.DbId;
        }
	}
}
