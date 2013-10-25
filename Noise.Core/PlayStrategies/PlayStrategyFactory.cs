using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies {
	public class PlayStrategyFactory : IPlayStrategyFactory {
		private readonly List<IPlayStrategy>	mStrategies; 

		public PlayStrategyFactory( IEnumerable<IPlayStrategy> strategyList  ) {
			mStrategies = new List<IPlayStrategy>( strategyList );
		}

	    public IEnumerable<IPlayStrategy> AvailableStrategies {
            get {  return( mStrategies ); }
	    } 

		public IPlayStrategy ProvidePlayStrategy( ePlayStrategy strategyId ) {
			IPlayStrategy	retValue = ( from strategy in mStrategies
												 where strategy.StrategyId == strategyId 
												 select  strategy ).FirstOrDefault();

			Condition.Requires( retValue ).IsNotNull();

			return( retValue );
		}
	}
}
