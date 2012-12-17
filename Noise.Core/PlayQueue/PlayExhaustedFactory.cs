using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	internal class PlayExhaustedFactory : IPlayExhaustedFactory {
		private readonly IPlayExhaustedStrategy		mStrategyStop;
		private readonly IPlayExhaustedStrategy		mStrategyReplay;
		private readonly IPlayExhaustedStrategy		mStrategyPlayList;
		private readonly IPlayExhaustedStrategy		mStrategyFavorites;
		private readonly IPlayExhaustedStrategy		mStrategySimilar;
		private readonly IPlayExhaustedStrategy		mStrategyStream;
		private readonly IPlayExhaustedStrategy		mStrategyArtist;
		private readonly IPlayExhaustedStrategy		mStrategyGenre;
		private readonly IPlayExhaustedStrategy		mStrategyCategory;

		public PlayExhaustedFactory( PlayExhaustedStrategyStop strategyStop,
 									 PlayExhaustedStrategyReplay strategyReplay,
									 PlayExhaustedStrategyPlayList strategyPlayList,
									 PlayExhaustedStrategyFavorites strategyFavorites,
									 PlayExhaustedStrategySimilar strategySimilar,
									 PlayExhaustedStrategyStream strategyStream,
									 PlayExhaustedStrategyArtist strategyArtist,
									 PlayExhaustedStrategyGenre strategyGenre,
									 PlayExhaustedStrategyCategory strategyCategory ) {
			mStrategyStop = strategyStop;
			mStrategyReplay = strategyReplay;
			mStrategyPlayList = strategyPlayList;
			mStrategyFavorites = strategyFavorites;
			mStrategySimilar = strategySimilar;
			mStrategyStream = strategyStream;
			mStrategyArtist = strategyArtist;
			mStrategyGenre = strategyGenre;
			mStrategyCategory = strategyCategory;
		}

		public IPlayExhaustedStrategy ProvideExhaustedStrategy( ePlayExhaustedStrategy strategy ) {
			IPlayExhaustedStrategy	retValue = null;

			switch( strategy ) {
				case ePlayExhaustedStrategy.Stop:
					retValue = mStrategyStop;
					break;

				case ePlayExhaustedStrategy.Replay:
					retValue = mStrategyReplay;
					break;

				case ePlayExhaustedStrategy.PlayList:
					retValue = mStrategyPlayList;
					break;

				case ePlayExhaustedStrategy.PlayFavorites:
					retValue = mStrategyFavorites;
					break;

				case ePlayExhaustedStrategy.PlaySimilar:
					retValue = mStrategySimilar;
					break;

				case ePlayExhaustedStrategy.PlayStream:
					retValue = mStrategyStream;
					break;

				case ePlayExhaustedStrategy.PlayArtist:
					retValue = mStrategyArtist;
					break;

				case ePlayExhaustedStrategy.PlayGenre:
					retValue = mStrategyGenre;
					break;

				case ePlayExhaustedStrategy.PlayCategory:
					retValue = mStrategyCategory;
					break;
			}

			return( retValue );
		}
	}
}
