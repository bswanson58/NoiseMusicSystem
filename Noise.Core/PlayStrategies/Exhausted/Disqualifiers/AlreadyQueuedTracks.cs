using System.Linq;
using Noise.Infrastructure.Dto;

namespace Noise.Core.PlayStrategies.Exhausted.Disqualifiers {
    class AlreadyQueuedTracks : IExhaustedPlayHandler {
        public string HandlerEnum => eTrackPlayDisqualifiers.AlreadyQueuedTracks.ToString();

        public void SelectTrack( IExhaustedSelectionContext context ) {
            var tracks = context.SelectedTracks.ToArray();

            foreach( var track in tracks ) {
                if( context.PlayQueue.IsTrackQueued( track )) {
                    context.SelectedTracks.Remove( track );
                }
            }
        }
    }
}
