using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class StrategyInformationResult : BaseResult {
		[DataMember]
		public int					CurrentPlayStrategy { get; set; }
		[DataMember]
		public long					PlayStrategyParameter { get; set; }
		[DataMember]
		public int					CurrentExhaustedStrategy { get; set; }
		[DataMember]
		public long					ExhaustedStrategyParameter { get; set; }
		[DataMember]
		public RoQueueStrategy[]	PlayStrategies { get; set; }
		[DataMember]
		public RoQueueStrategy[]	ExhaustedStrategies { get; set; }
		[DataMember]
		public RoStrategyParameter[] GenreParameters { get; set; }
	}
}
