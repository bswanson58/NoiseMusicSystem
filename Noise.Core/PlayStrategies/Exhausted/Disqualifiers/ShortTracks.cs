using System;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using ReusableBits.ExtensionClasses.MoreLinq;

namespace Noise.Core.PlayStrategies.Exhausted.Disqualifiers {
    public class ShortTracks : ExhaustedHandlerBase {
        public ShortTracks() : base( eTrackPlayHandlers.ShortTracks, eTrackPlayStrategy.Disqualifier, "Short Tracks", "Don't suggest short tracks." ) { }

        public override void SelectTrack( IExhaustedSelectionContext context ) {
            var tracks = context.SelectedTracks;
            var minTrackTime = TimeSpan.FromSeconds( 10 );

            tracks.ForEach( 
                track => {
                    if( track.Duration < minTrackTime ) {
                        context.SelectedTracks.Remove( track );
                    }
                });
        }
    }
}
