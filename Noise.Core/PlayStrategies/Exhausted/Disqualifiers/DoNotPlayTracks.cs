using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using ReusableBits.ExtensionClasses.MoreLinq;

namespace Noise.Core.PlayStrategies.Exhausted.Disqualifiers {
    public class DoNotPlayTracks : ExhaustedHandlerBase {
        public DoNotPlayTracks() : base( eTrackPlayHandlers.DoNotPlayTracks, eTrackPlayStrategy.Disqualifier, "Do Not Play Tracks", "Eliminate tracks marked as not to be suggested." ) { }

        public override void SelectTrack( IExhaustedSelectionContext context ) {
            var tracks = context.SelectedTracks;

            tracks.ForEach( 
                track => {
                    if( track.DoNotStrategyPlay ) {
                        context.SelectedTracks.Remove( track );
                    }
                });
        }
    }
}
