using System.Runtime.Serialization;
using Noise.Infrastructure.Interfaces;

namespace Noise.Infrastructure.RemoteDto {
	[DataContract]
	public class RoQueueStrategy {
		[DataMember]
		public int			StrategyId { get; set; }
		[DataMember]
		public string		StrategyName { get; set; }
		[DataMember]
		public string		StrategyDescription { get; set; }
		[DataMember]
		public bool			RequiresParameter { get; set; }
		[DataMember]
		public int			ParameterType { get; set; }
		[DataMember]
		public string		ParameterTitle { get; set; }

		public RoQueueStrategy( IPlayStrategy strategy ) {
			StrategyId = (int)strategy.StrategyId;
			StrategyName = strategy.StrategyName;
			StrategyDescription = strategy.StrategyDescription;
			RequiresParameter = strategy.RequiresParameters;
			ParameterTitle = strategy.ParameterName;
		}

		public RoQueueStrategy( IPlayExhaustedStrategy strategy ) {
			StrategyId = (int)strategy.StrategyId;
			StrategyName = strategy.StrategyName;
			StrategyDescription = strategy.StrategyDescription;
			RequiresParameter = strategy.RequiresParameters;
			ParameterTitle = strategy.ParameterName;
		}
	}
}
