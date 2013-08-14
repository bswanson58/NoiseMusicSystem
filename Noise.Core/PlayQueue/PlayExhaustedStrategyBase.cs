using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	public abstract class PlayExhaustedStrategyBase : IPlayExhaustedStrategy {
		private readonly ePlayExhaustedStrategy		mStrategy;

		protected abstract DbTrack SelectATrack();

		protected PlayExhaustedStrategyBase( ePlayExhaustedStrategy strategy ) {
			mStrategy = strategy;
		}

		public ePlayExhaustedStrategy PlayStrategy {
			get{ return( mStrategy ); }
		}

		protected virtual void ProcessParameters( IPlayStrategyParameters parameters ) { }

		public bool QueueTracks( IPlayQueue queueMgr, IPlayStrategyParameters parameters ) {
			var retValue = false;
			var circuitBreaker = 25;

			ProcessParameters( parameters );

			while( circuitBreaker > 0 ) {
				var track = SelectATrack();

				if(( track != null ) &&
				   (!queueMgr.IsTrackQueued( track )) &&
				   ( track.Rating >= 0 )) {
					queueMgr.StrategyAdd( track );

					retValue = true;
					break;
				}

				circuitBreaker--;
			}

			return ( retValue );
		}
	}
}
