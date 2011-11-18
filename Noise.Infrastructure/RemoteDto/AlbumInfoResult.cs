using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class AlbumInfoResult : BaseResult {
		[DataMember]
		public RoAlbumInfo	AlbumInfo { get; set; }
	}
}
