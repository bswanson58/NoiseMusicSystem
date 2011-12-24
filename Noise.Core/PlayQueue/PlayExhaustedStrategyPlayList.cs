using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	internal class PlayExhaustedStrategyPlayList : IPlayExhaustedStrategy {
		private readonly	IPlayListProvider	mPlayListProvider;
		private	readonly	ITrackProvider		mTrackProvider;

		public PlayExhaustedStrategyPlayList( IPlayListProvider playListProvider, ITrackProvider trackProvider ) {
			mPlayListProvider = playListProvider;
			mTrackProvider = trackProvider;
		}

		public bool QueueExhausted( IPlayQueue queueMgr, long itemId ) {
			var retValue = false;
			var	playList = mPlayListProvider.GetPlayList( itemId );

			if( playList != null ) {
				var tracks = mTrackProvider.GetTrackListForPlayList( playList );

				foreach( var track in tracks ) {
					if(!queueMgr.IsTrackQueued( track )) {
						queueMgr.Add( track );

						retValue = true;
					}
				}
			}
			return( retValue );
		}

		public void NextTrackPlayed() {
		}
	}
}
