using System.Runtime.Serialization;
using Noise.Infrastructure.Interfaces;

namespace Noise.Infrastructure.RemoteDto {
    public enum RoStrategyParameterType {
        Artist = 1,
        Genre = 2,
		Category = 3
    }

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

			if( strategy.StrategyId == ePlayStrategy.FeaturedArtists ) {
				ParameterType = (int)RoStrategyParameterType.Artist;
			}
		}

		public RoQueueStrategy( IPlayExhaustedStrategy strategy ) {
			StrategyId = (int)strategy.StrategyId;
			StrategyName = strategy.StrategyName;
			StrategyDescription = strategy.StrategyDescription;
			RequiresParameter = strategy.RequiresParameters;
			ParameterTitle = strategy.ParameterName;

			switch( strategy.StrategyId ) {
				case ePlayExhaustedStrategy.PlayArtist:
					ParameterType = (int)RoStrategyParameterType.Artist;
					break;

				case ePlayExhaustedStrategy.PlayArtistGenre:
				case ePlayExhaustedStrategy.PlayGenre:
					ParameterType = (int)RoStrategyParameterType.Genre;
					break;

				case ePlayExhaustedStrategy.PlayCategory:
					ParameterType = (int)RoStrategyParameterType.Category;
					break;
			}
		}
	}
}
