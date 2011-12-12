using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	internal class PlayStrategyFactory : IPlayStrategyFactory {
		private readonly IPlayStrategy	mPlayStrategySingle;
		private readonly IPlayStrategy	mPlayStrategyRandom;
		private readonly IPlayStrategy	mPlayStrategyTwoFers;

		public PlayStrategyFactory( PlayStrategySingle singleStrategy, PlayStrategyRandom randomStrategy, PlayStrategyTwoFers twoFerStrategy ) {
			mPlayStrategySingle = singleStrategy;
			mPlayStrategyRandom = randomStrategy;
			mPlayStrategyTwoFers = twoFerStrategy;
		}

		public IPlayStrategy ProvidePlayStrategy( ePlayStrategy strategy ) {
			IPlayStrategy	retValue = null;

			switch( strategy ) {
				case ePlayStrategy.Next:
					retValue = mPlayStrategySingle;
					break;

				case ePlayStrategy.Random:
					retValue = mPlayStrategyRandom;
					break;

				case ePlayStrategy.TwoFers:
					retValue = mPlayStrategyTwoFers;
					break;
			}

			return( retValue );
		}
	}
}
