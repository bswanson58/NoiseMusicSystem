using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	internal class PlayExhaustedStrategyPlayList : IPlayExhaustedStrategy {
		private	readonly	IPlayListMgr	mPlayListMgr;

		public PlayExhaustedStrategyPlayList( IPlayListMgr playListManager ) {
			mPlayListMgr = playListManager;
		}

		public bool QueueExhausted( IPlayQueue queueMgr, long itemId ) {
			var retValue = false;
			var	playList = mPlayListMgr.GetPlayList( itemId );

			if( playList != null ) {
				var tracks = mPlayListMgr.GetTracks( playList );

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
