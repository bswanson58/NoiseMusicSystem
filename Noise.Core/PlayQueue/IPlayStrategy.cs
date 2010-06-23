using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Core.PlayQueue {
	internal interface IPlayStrategy {
		PlayQueueTrack	NextTrack( IList<PlayQueueTrack> queue );
	}
}
