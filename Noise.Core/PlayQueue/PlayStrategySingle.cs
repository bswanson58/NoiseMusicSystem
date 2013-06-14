using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	internal class PlayStrategySingle : IPlayStrategy {
		public PlayQueueTrack NextTrack( IPlayQueue queueMgr, IList<PlayQueueTrack> queue, IPlayStrategyParameters parameters ) {
			return( queue.FirstOrDefault( track => ( !track.IsPlaying ) && ( !track.HasPlayed )));
		}
	}
}
