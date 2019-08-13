using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies.Exhausted.Disqualifiers {
    class AlreadyQueuedTracks : ExhaustedHandlerBase {
        public AlreadyQueuedTracks() : 
            base ( eTrackPlayHandlers.AlreadyQueuedTracks, eTrackPlayStrategy.Disqualifier, 
                   "Prevent previously queued tracks", "Do not select tracks already in the play queue." ) { }

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
