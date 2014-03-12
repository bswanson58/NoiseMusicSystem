using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class RoStrategyInformation {
		[DataMember]
		public int						PlayStrategy { get; set; }
		[DataMember]
		public long						PlayStrategyParameter { get; set; }
		[DataMember]
		public int						ExhaustedStrategy { get; set; }
		[DataMember]
		public long						ExhaustedStrategyParameter { get; set; }
		[DataMember]
		public RoQueueStrategy[]		PlayStrategies { get; set; }
		[DataMember]
		public RoQueueStrategy[]		ExhaustedStrategies { get; set; }
		[DataMember]
		public RoStrategyParameter[]	GenreParameters { get; set; }
		[DataMember]
		public RoStrategyParameter[]	ArtistParameters { get; set; }

		public RoStrategyInformation() {
			PlayStrategies = new RoQueueStrategy[0];
			ExhaustedStrategies = new RoQueueStrategy[0];
			GenreParameters = new RoStrategyParameter[0];
		}
	}
}
