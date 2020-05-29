using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using ReusableBits.ExtensionClasses.MoreLinq;

namespace Noise.Core.PlayStrategies.Exhausted.Disqualifiers {
    public class BadRatingTracks : ExhaustedHandlerBase {
        public BadRatingTracks() : base( eTrackPlayHandlers.BadRatingTracks, eTrackPlayStrategy.Disqualifier, "Bad Rating", "Eliminate tracks with a bad rating." ) { }

        public override void SelectTrack( IExhaustedSelectionContext context ) {
            var tracks = context.SelectedTracks;

            tracks.ForEach( 
                track => {
                    if( track.Rating < 0 ) {
                        context.SelectedTracks.Remove( track );
                    }
                });
        }
    }
}
