using System.Collections.Generic;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	internal interface IPlayStrategy {
		PlayQueueTrack	NextTrack( IPlayQueue queueMgr, IList<PlayQueueTrack> queue, IPlayStrategyParameters parameters );
	}
}
