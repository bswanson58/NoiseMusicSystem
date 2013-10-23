using CuttingEdge.Conditions;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies {
	public abstract class PlayExhaustedStrategyBase : IPlayExhaustedStrategy {
		private		readonly ePlayExhaustedStrategy	mStrategy;
		private		readonly string					mDisplayName;
		private		readonly bool					mParametersRequired;
		protected	IPlayQueue						mQueueMgr;
		protected	IPlayStrategyParameters			mParameters;

		protected abstract DbTrack SelectATrack();
		protected abstract string FormatDescription();

		protected PlayExhaustedStrategyBase( ePlayExhaustedStrategy strategy, string displayName, bool parametersRequired ) {
			mStrategy = strategy;
			mDisplayName = displayName;
			mParametersRequired = parametersRequired;
		}

		public ePlayExhaustedStrategy StrategyId {
			get{ return( mStrategy ); }
		}

		public string DisplayName {
			get{ return( mDisplayName ); }
		}

		public string StrategyDescription {
			get {  return( FormatDescription()); }
		}

		public bool	RequiresParameters {
			get{ return( mParametersRequired ); }
		}

		public bool Initialize( IPlayQueue queueMgr, IPlayStrategyParameters parameters ) {
			mQueueMgr = queueMgr;
			mParameters = parameters;

			Condition.Requires( mQueueMgr ).IsNotNull();
			if( mParametersRequired ) {
				Condition.Requires( mParameters ).IsOfType( typeof( PlayStrategyParameterDbId ));

				ProcessParameters( mParameters );
			}

			return( true );
		}

		protected virtual void ProcessParameters( IPlayStrategyParameters parameters ) { }

		public virtual bool QueueTracks() {
			var retValue = false;
			var circuitBreaker = 25;

			while(( circuitBreaker > 0 ) &&
				  ( mQueueMgr.UnplayedTrackCount < 3 )) {
				var track = SelectATrack();

				if(( track != null ) &&
					(!mQueueMgr.IsTrackQueued( track )) &&
					( track.Rating >= 0 )) {
					mQueueMgr.StrategyAdd( track );

					retValue = true;
				}

				circuitBreaker--;
			}

			return ( retValue );
		}
	}
}
