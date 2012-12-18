using System;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	internal class PlayExhaustedStrategyFavorites : PlayExhaustedListBase {
		private readonly	ITrackProvider	mTrackProvider;

		public PlayExhaustedStrategyFavorites( ITrackProvider trackProvider ) :
			base( ePlayExhaustedStrategy.PlayFavorites ) {
			mTrackProvider = trackProvider;
		}

		protected override void FillTrackList( long itemId ) {
			mTrackList.Clear();

			try {
				using( var list = mTrackProvider.GetFavoriteTracks()) {
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
