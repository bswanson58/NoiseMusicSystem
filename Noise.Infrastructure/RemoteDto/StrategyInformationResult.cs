using System.Runtime.Serialization;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class StrategyInformationResult : BaseResult {
		[DataMember]
		public RoStrategyInformation	StrategyInformation { get; set; }

		public StrategyInformationResult() {
			StrategyInformation = new RoStrategyInformation();
		}
	}
}
