using System;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies {
	internal class PlayStrategyRandom : PlayStrategyBase {
		public PlayStrategyRandom() :
			base( ePlayStrategy.Random, "Random", "Plays selected tracks from the queue in random order." ) {
		}

	    protected override string FormatDescription() {
            return( "in random order" );
	    }

		public override PlayQueueTrack NextTrack() {
			PlayQueueTrack	retValue = null;

			var eligibleTracks = ( from PlayQueueTrack track in PlayQueueMgr.PlayList where !track.HasPlayed && !track.IsPlaying select track ).ToList();

			if( eligibleTracks.Any()) {
				var r = new Random( DateTime.Now.Millisecond );
				var next = r.Next( eligibleTracks.Count - 1 );

				retValue = eligibleTracks.Skip( next ).FirstOrDefault();
			}
			
			return ( retValue );
		}
	}
}
