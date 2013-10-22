using System;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies {
	internal class PlayStrategyRandom : IPlayStrategy {
        private IPlayQueue  mPlayQueue;

	    public ePlayStrategy StrategyId {
            get {  return( ePlayStrategy.Random ); }
	    }
	    public string DisplayName {
            get {  return( "Random" ); }
	    }

	    public string StrategyDescription {
            get {  return( "" ); }
	    }

	    public bool RequiresParameters {
            get {  return( false ); }
	    }

	    public bool Initialize( IPlayQueue queueMgr, IPlayStrategyParameters parameters ) {
            mPlayQueue = queueMgr;

            return( true );
	    }

		public PlayQueueTrack NextTrack() {
			PlayQueueTrack	retValue = null;

			var eligibleTracks = ( from PlayQueueTrack track in mPlayQueue.PlayList where !track.HasPlayed && !track.IsPlaying select track ).ToList();

			if( eligibleTracks.Any()) {
				var r = new Random( DateTime.Now.Millisecond );
				var next = r.Next( eligibleTracks.Count - 1 );

				retValue = eligibleTracks.Skip( next ).FirstOrDefault();
			}
			
			return ( retValue );
		}
	}
}
