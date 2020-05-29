using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies.Exhausted.Disqualifiers {
    public class AlreadyQueuedTracks : ExhaustedHandlerBase {
        public AlreadyQueuedTracks() : 
            base ( eTrackPlayHandlers.AlreadyQueuedTracks, eTrackPlayStrategy.Disqualifier, 
                   "Prevent previously queued tracks", "Don't suggest tracks already in the play queue." ) { }

        public override void SelectTrack( IExhaustedSelectionContext context ) {
            var tracks = context.SelectedTracks.ToArray();

            foreach( var track in tracks ) {
                if( context.PlayQueue.IsTrackQueued( track )) {
                    context.SelectedTracks.Remove( track );
                }
            }
        }
    }
}
