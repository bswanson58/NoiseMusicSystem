using System;
using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	internal class PlayStrategyRandom : IPlayStrategy {
		public PlayQueueTrack NextTrack( IPlayQueue queueMgr, IList<PlayQueueTrack> queue, IPlayStrategyParameters parameters ) {
			PlayQueueTrack	retValue = null;

			var eligibleTracks = ( from PlayQueueTrack track in queue where !track.HasPlayed && !track.IsPlaying select track ).ToList();

			if( eligibleTracks.Any()) {
				var r = new Random( DateTime.Now.Millisecond );
				var next = r.Next( eligibleTracks.Count - 1 );

				retValue = eligibleTracks.Skip( next ).FirstOrDefault();
			}
			
			return ( retValue );
		}
	}
}
