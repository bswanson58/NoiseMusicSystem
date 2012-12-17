using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	internal class PlayExhaustedStrategyReplay : IPlayExhaustedStrategy {
		public ePlayExhaustedStrategy PlayStrategy {
			get{ return( ePlayExhaustedStrategy.Replay ); }
		}

		public bool QueueExhausted( IPlayQueue queueMgr, long itemId ) {
			var retValue = false;

			foreach( var track in queueMgr.PlayList ) {
				if(!track.IsPlaying ) {
					track.HasPlayed = false;

					retValue = true;
				}
			}

			return( retValue );
		}

		public void NextTrackPlayed() {
		}
	}
}
