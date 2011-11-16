using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class RoBase {
		[DataMember]
		public long	DbId { get; set; }
	}
}
