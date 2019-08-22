using System.Runtime.Serialization;
using Noise.Infrastructure.Dto;
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

		public RoQueueStrategy( IStrategyDescription strategy ) {
			StrategyId = (int)strategy.Identifier;
			StrategyName = strategy.Name;
			StrategyDescription = strategy.Description;
			RequiresParameter = strategy.RequiresParameters;
//			ParameterTitle = strategy.ParameterName;

			switch( strategy.Identifier ) {
				case eTrackPlayHandlers.PlayArtist:
					ParameterType = (int)RoStrategyParameterType.Artist;
					break;

				case eTrackPlayHandlers.PlayGenre:
					ParameterType = (int)RoStrategyParameterType.Genre;
					break;

//				case eTrackPlayHandlers.PlayCategory:
//					ParameterType = (int)RoStrategyParameterType.Category;
//					break;
			}
		}

	}
}
