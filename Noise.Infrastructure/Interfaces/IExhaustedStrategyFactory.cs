using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
    public interface IExhaustedStrategyFactory {
        IEnumerable<IStrategyDescription>   ExhaustedStrategies { get; }
        IStrategyDescription                StrategyDescription( ePlayExhaustedStrategy forStrategy );

        void    BuildStrategy( ExhaustedStrategySpecification specification,
                               IList<IExhaustedPlayHandler> suggesters, IList<IExhaustedPlayHandler> disqualifiers, IList<IExhaustedPlayHandler> bonusHandlers );
    }
}
