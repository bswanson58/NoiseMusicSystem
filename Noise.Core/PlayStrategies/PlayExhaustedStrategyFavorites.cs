using System;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies {
	internal class PlayExhaustedStrategyFavorites : PlayExhaustedListBase {
		private readonly	ITrackProvider	mTrackProvider;

		public PlayExhaustedStrategyFavorites( ITrackProvider trackProvider ) :
			base( ePlayExhaustedStrategy.PlayFavorites, "Play Favorites", "Play tracks from your list of favorites." ) {
			mTrackProvider = trackProvider;
		}

		protected override string FormatDescription() {
			return( string.Format( "play favorite tracks" ));
		}

		protected override void FillTrackList( long itemId ) {
			mTrackList.Clear();

			try {
				using( var list = mTrackProvider.GetFavoriteTracks()) {
					foreach( var track in list.List ) {
						if(!PlayQueueMgr.IsTrackQueued( track )) {
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
