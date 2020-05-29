using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using ReusableBits.ExtensionClasses.MoreLinq;

namespace Noise.Core.PlayStrategies.Exhausted.Suggesters {
    public class ReplayQueue : ExhaustedHandlerBase {
        public ReplayQueue() : base( eTrackPlayHandlers.Replay, eTrackPlayStrategy.Suggester, "Replay Queue", "replay the tracks in the play queue" ) { }

        public override void SelectTrack( IExhaustedSelectionContext context ) {
            if( context.PlayQueue.UnplayedTrackCount == 0 ) {
                context.PlayQueue.PlayList.ForEach( 
                    track => {
                        if(!track.IsPlaying) {
                            track.HasPlayed = false;
                        }
                    });
            }
        }
    }
}
