using CuttingEdge.Conditions;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies {
	public abstract class PlayStrategyBase : IPlayStrategy {
		private readonly ePlayStrategy	mStrategy;
		private readonly string			mStrategyName;
		private readonly string			mStrategyDescription;
		private readonly bool			mParametersRequired;
		private readonly string			mParameterName;
		private IPlayQueue				mQueueMgr;
		private IPlayStrategyParameters	mParameters;

		protected abstract string		FormatDescription();
        protected IPlayQueue			PlayQueueMgr => mQueueMgr;

        public	ePlayStrategy			StrategyId => mStrategy;
        public	string					StrategyName => mStrategyName;
        public	string					StrategyDescription => mStrategyDescription;
        public	string					ConfiguredDescription => FormatDescription();
        public	bool					RequiresParameters => mParametersRequired;
        public	string					ParameterName => mParameterName;
        public	IPlayStrategyParameters Parameters => mParameters;


		protected PlayStrategyBase( ePlayStrategy strategy, string strategyName, string strategyDescription ) {
			mStrategy = strategy;
			mStrategyName = strategyName;
			mStrategyDescription = strategyDescription;
			mParametersRequired = false;
			mParameterName = string.Empty;
		}

		protected PlayStrategyBase( ePlayStrategy strategy, string strategyName, string strategyDescription, string parameterName ) :
			this( strategy, strategyName, strategyDescription ) {
			mParametersRequired = true;
			mParameterName = parameterName;
		}

        protected virtual void ProcessParameters( IPlayStrategyParameters parameters ) { }

		public virtual bool Initialize( IPlayQueue queueMgr, IPlayStrategyParameters parameters ) {
			mQueueMgr = queueMgr;
			mParameters = parameters;

			Condition.Requires( mQueueMgr ).IsNotNull();
			if( mParametersRequired ) {
				Condition.Requires( mParameters ).IsOfType( typeof( PlayStrategyParameterDbId ));

				ProcessParameters( mParameters );
			}

			return( true );
		}

		public virtual PlayQueueTrack NextTrack() {
			throw new System.NotImplementedException();
		}
	}
}
