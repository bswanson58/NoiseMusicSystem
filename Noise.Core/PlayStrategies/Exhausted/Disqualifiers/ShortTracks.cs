using System;
using System.Linq;
using Microsoft.Practices.ObjectBuilder2;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies.Exhausted.Disqualifiers {
    class ShortTracks : ExhaustedHandlerBase {
        public ShortTracks() : base( eTrackPlayHandlers.ShortTracks, eTrackPlayStrategy.Disqualifier, "Short Tracks", "Don't suggest short tracks." ) { }

        public override void SelectTrack( IExhaustedSelectionContext context ) {
            var tracks = context.SelectedTracks.ToArray();
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
