using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies.Exhausted {
    class ExhaustedStrategyFactory : IExhaustedStrategyFactory {
        private readonly List<IExhaustedPlayHandler>    mStrategies; 

        public IEnumerable<IStrategyDescription>        ExhaustedStrategies => mStrategies;

        public ExhaustedStrategyFactory( IExhaustedPlayHandler[] strategyList  ) {
            mStrategies = new List<IExhaustedPlayHandler>( strategyList );
        }

        public void BuildStrategy( ExhaustedStrategySpecification specification,
                                   IList<IExhaustedPlayHandler> suggesters, IList<IExhaustedPlayHandler> disqualifiers, IList<IExhaustedPlayHandler> bonusHandlers ) {
            suggesters.Clear();
            disqualifiers.Clear();
            bonusHandlers.Clear();

            foreach( var handler in specification.TrackSuggesters ) {
                suggesters.Add( mStrategies.FirstOrDefault( h => h.Identifier.Equals( handler )));
            }

            foreach( var handler in specification.TrackDisqualifiers ) {
                disqualifiers.Add( mStrategies.FirstOrDefault( h => h.Identifier.Equals( handler )));
            }

            foreach( var handler in specification.TrackBonusSuggesters ) {
                bonusHandlers.Add( mStrategies.FirstOrDefault( h => h.Identifier.Equals( handler )));
            }

            Condition.Requires( suggesters.All( h => h != null )).IsTrue( "Track play suggester not set." );
            Condition.Requires( disqualifiers.All( h => h != null )).IsTrue( "Track play disqualifier not set." );
            Condition.Requires( bonusHandlers.All( h => h != null )).IsTrue( "Track bonus handler not set." );
        }
    }
}
