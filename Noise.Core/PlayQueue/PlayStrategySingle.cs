using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;

namespace Noise.Core.PlayQueue {
	internal class PlayStrategySingle : IPlayStrategy {
		public PlayQueueTrack NextTrack( IList<PlayQueueTrack> queue ) {
			return( queue.FirstOrDefault( track => ( !track.IsPlaying ) && ( !track.HasPlayed )));
		}
	}
}
