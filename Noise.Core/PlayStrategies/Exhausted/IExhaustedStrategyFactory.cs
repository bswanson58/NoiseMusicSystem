using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Core.PlayStrategies.Exhausted {
    interface IExhaustedStrategyFactory {
        void    BuildStrategy( ExhaustedStrategySpecification specification,
                               IList<IExhaustedPlayHandler> suggestors, IList<IExhaustedPlayHandler> disqualifiers, IList<IExhaustedPlayHandler> bonusHandlers );
    }
}
