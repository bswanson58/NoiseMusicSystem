using System.Linq;
using Microsoft.Practices.ObjectBuilder2;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies.Exhausted.Disqualifiers {
    class BadRatingTracks : ExhaustedHandlerBase {
        public BadRatingTracks() : base( eTrackPlayHandlers.BadRatingTracks, eTrackPlayStrategy.Disqualifier, "Bad Rating", "Eliminate tracks with a bad rating." ) { }

        public override void SelectTrack( IExhaustedSelectionContext context ) {
            var tracks = context.SelectedTracks.ToArray();

            tracks.ForEach( 
                track => {
                    if( track.Rating < 0 ) {
                        context.SelectedTracks.Remove( track );
                    }
                });
        }
    }
}
