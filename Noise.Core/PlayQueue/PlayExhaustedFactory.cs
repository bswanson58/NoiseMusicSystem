using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	internal class PlayExhaustedFactory : IPlayExhaustedFactory {
		private readonly List<IPlayExhaustedStrategy>	mStrategies; 

		public PlayExhaustedFactory( IEnumerable<IPlayExhaustedStrategy> strategyList  ) {
			mStrategies = new List<IPlayExhaustedStrategy>( strategyList );
		}

		public IPlayExhaustedStrategy ProvideExhaustedStrategy( ePlayExhaustedStrategy playStrategy ) {
			IPlayExhaustedStrategy	retValue = ( from strategy in mStrategies
												 where strategy.PlayStrategy == playStrategy 
												 select  strategy ).FirstOrDefault();

			Condition.Requires( retValue ).IsNotNull();

			return( retValue );
		}
	}
}
