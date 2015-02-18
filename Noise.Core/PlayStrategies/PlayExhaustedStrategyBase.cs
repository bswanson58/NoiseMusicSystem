using CuttingEdge.Conditions;
using Noise.Core.Logging;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies {
	public abstract class PlayExhaustedStrategyBase : IPlayExhaustedStrategy {
		private	readonly ePlayExhaustedStrategy	mStrategy;
		private	readonly string					mStrategyName;
		private	readonly string					mStrategyDescription;
		private	readonly string					mParameterName;
		private	readonly bool					mParametersRequired;
		private	readonly ILogPlayStrategy		mLog;
		private	IPlayStrategyParameters			mParameters;
		private	IPlayQueue						mQueueMgr;

		protected abstract DbTrack SelectATrack();
		protected abstract string FormatDescription();

		protected PlayExhaustedStrategyBase( ePlayExhaustedStrategy strategy, string strategyName, string strategyDescription, ILogPlayStrategy log ) {
			mStrategy = strategy;
			mStrategyName = strategyName;
			mStrategyDescription = strategyDescription;
			mLog = log;
			mParametersRequired = false;
			mParameterName = string.Empty;
		}

		protected PlayExhaustedStrategyBase( ePlayExhaustedStrategy strategy, string strategyName, string strategyDeecription, string parameterName, ILogPlayStrategy log ) :
			this( strategy, strategyName, strategyDeecription, log ) {
			mParametersRequired = true;
			mParameterName = parameterName;			
		}

		protected ILogPlayStrategy Log {
			get { return( mLog ); }
		}

		protected void LogTrackAdded( DbTrack track ) {
			Log.LogTrackQueued( mStrategy, track );
		}

		public ePlayExhaustedStrategy StrategyId {
			get{ return( mStrategy ); }
		}

		public string StrategyName {
			get{ return( mStrategyName ); }
		}

		public string StrategyDescription {
			get {  return( mStrategyDescription ); }
		}

		public string ConfiguredDescription {
			get {  return( FormatDescription()); }
		}

		public bool	RequiresParameters {
			get{ return( mParametersRequired ); }
		}

		public string ParameterName {
			get {  return( mParameterName ); }
		}

		public IPlayStrategyParameters Parameters {
			get { return( mParameters ); }
		}

		protected IPlayQueue PlayQueueMgr {
			get {  return( mQueueMgr ); }
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

					LogTrackAdded( track );
					retValue = true;
				}

				circuitBreaker--;
			}

			return ( retValue );
		}
	}
}
