using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
    public interface IExhaustedStrategyPlayManager {
        void                    SetExhaustedStrategy( ExhaustedStrategySpecification specification );
        IEnumerable<DbTrack>    SelectTracks( IPlayQueue playQueue, int minCount );
    }
}
