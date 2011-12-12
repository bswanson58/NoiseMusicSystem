using System;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	internal class PlayExhaustedStrategyFavorites : PlayExhaustedListBase {
		private readonly	IDataProvider	mDataProvider;

		public PlayExhaustedStrategyFavorites( IDataProvider dataProvider ) {
			mDataProvider = dataProvider;
		}

		protected override void FillTrackList( long itemId ) {
			mTrackList.Clear();

			try {
				using( var list = mDataProvider.GetFavoriteTracks()) {
					foreach( var track in list.List ) {
						if(!mQueueMgr.IsTrackQueued( track )) {
							mTrackList.Add( track );
						}
					}
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - PlayExhaustedStrategyFavorites: ", ex );
			}
		}
	}
}
