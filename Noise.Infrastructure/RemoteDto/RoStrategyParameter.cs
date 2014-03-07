using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	public enum RoStrategyParameterType {
		Artist = 1,
		Genre = 2,
	}

	[DataContract]
	public class RoStrategyParameter {
		[DataMember]
		public int		Type { get; set; }
		[DataMember]
		public long		Id { get; set; }
		[DataMember]
		public string	Title { get; set; }
	}
}
