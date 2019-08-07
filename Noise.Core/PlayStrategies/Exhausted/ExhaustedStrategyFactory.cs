using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.Infrastructure.Dto;

namespace Noise.Core.PlayStrategies.Exhausted {
    class ExhaustedStrategyFactory : IExhaustedStrategyFactory {
        private readonly List<IExhaustedPlayHandler>    mStrategies; 

        public ExhaustedStrategyFactory( IEnumerable<IExhaustedPlayHandler> strategyList  ) {
            mStrategies = new List<IExhaustedPlayHandler>( strategyList );
        }

        public void BuildStrategy( ExhaustedStrategySpecification specification,
                                   IList<IExhaustedPlayHandler> suggestors, IList<IExhaustedPlayHandler> disqualifiers, IList<IExhaustedPlayHandler> bonusHandlers ) {
            suggestors.Clear();
            disqualifiers.Clear();
            bonusHandlers.Clear();

            foreach( var handler in specification.TrackSuggesters ) {
                suggestors.Add( mStrategies.FirstOrDefault( h => h.HandlerEnum.Equals( handler.ToString())));
            }

            foreach( var handler in specification.TrackDisqualifiers ) {
                disqualifiers.Add( mStrategies.FirstOrDefault( h => h.HandlerEnum.Equals( handler.ToString())));
            }

            foreach( var handler in specification.TrackBonusSuggesters ) {
                bonusHandlers.Add( mStrategies.FirstOrDefault( h => h.HandlerEnum.Equals( handler.ToString())));
            }

            Condition.Requires( suggestors.All( h => h != null )).IsTrue( "Track play suggester not set." );
            Condition.Requires( disqualifiers.All( h => h != null )).IsTrue( "Track play disqualifier not set." );
            Condition.Requires( bonusHandlers.All( h => h != null )).IsTrue( "Track bonus handler not set." );
        }
    }
}
