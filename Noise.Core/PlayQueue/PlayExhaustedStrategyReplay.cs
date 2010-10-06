using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	internal class PlayQueueExhaustedStrategyReplay : IPlayExhaustedStrategy {
		public bool QueueExhausted( IPlayQueue queueMgr ) {
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
