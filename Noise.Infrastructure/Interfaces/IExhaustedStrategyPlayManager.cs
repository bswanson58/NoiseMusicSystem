using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
    public interface IExhaustedStrategyPlayManager {
        ExhaustedStrategySpecification  StrategySpecification { get; set; }
        IStrategyDescription            CurrentStrategy { get; }

        IEnumerable<DbTrack>            SelectTracks( IPlayQueue playQueue, int minCount );
    }
}
