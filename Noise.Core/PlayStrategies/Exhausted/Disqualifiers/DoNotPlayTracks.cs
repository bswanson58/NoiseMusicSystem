using System.Linq;
using Microsoft.Practices.ObjectBuilder2;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies.Exhausted.Disqualifiers {
    class DoNotPlayTracks : ExhaustedHandlerBase {
        public DoNotPlayTracks() : base( eTrackPlayHandlers.DoNotPlayTracks, eTrackPlayStrategy.Disqualifier, "Do Not Play Tracks", "Eliminate tracks marked as not to be suggested" ) { }

        public override void SelectTrack( IExhaustedSelectionContext context ) {
            var tracks = context.SelectedTracks.ToArray();

            tracks.ForEach( 
                track => {
                    if( track.DoNotStrategyPlay ) {
                        context.SelectedTracks.Remove( track );
                    }
                });
        }
    }
}
