using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies {
	public class PlayStrategySingle : PlayStrategyBase {
		public PlayStrategySingle() :
			base( ePlayStrategy.Next, "Normal", "Plays tracks from the play queue in sequential order." ) {
		}

	    protected override string FormatDescription() {
            return( "in sequential order" );
	    }

	    public override PlayQueueTrack NextTrack() {
			return( PlayQueueMgr.PlayList.FirstOrDefault( track => ( !track.IsPlaying ) && ( !track.HasPlayed )));
	    }
	}
}
