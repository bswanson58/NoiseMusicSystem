using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class RoStrategyParameter {
		[DataMember]
		public long		Id { get; set; }
		[DataMember]
		public string	Title { get; set; }
	}
}
