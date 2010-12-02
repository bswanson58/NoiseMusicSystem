using System;
using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	internal class PlayStrategyRandom : IPlayStrategy {
		public PlayQueueTrack NextTrack( IPlayQueue queueMgr, IList<PlayQueueTrack> queue ) {
			PlayQueueTrack	retValue = null;

			var eligibleTracks = from PlayQueueTrack track in queue where !track.HasPlayed && !track.IsPlaying select track;
			var trackCount = eligibleTracks.Count();

			if( trackCount > 0 ) {
				var r = new Random( DateTime.Now.Millisecond );
				var next = r.Next( trackCount - 1 );

				retValue = eligibleTracks.Skip( next ).FirstOrDefault();
			}
			
			return ( retValue );
		}
	}
}
