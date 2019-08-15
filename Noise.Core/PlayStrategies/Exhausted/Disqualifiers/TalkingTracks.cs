using System.Linq;
using Microsoft.Practices.ObjectBuilder2;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies.Exhausted.Disqualifiers {
    class TalkingTracks : ExhaustedHandlerBase {
        public TalkingTracks() : base( eTrackPlayHandlers.TalkingTracks, eTrackPlayStrategy.Disqualifier, "Talking Tracks", "Tracks that are named to be chatty" ) { }

        public override void SelectTrack( IExhaustedSelectionContext context ) {
            var tracks = context.SelectedTracks.ToArray();

            tracks.ForEach( 
                track => {
                    if(( track.Name.ToLower().Equals( "applause" )) ||
                       ( track.Name.ToLower().Equals( "banter" )) ||
                       ( track.Name.ToLower().Equals( "chat" )) ||
                       ( track.Name.ToLower().Equals( "talk" )) ||
                       ( track.Name.ToLower().Equals( "encore" )) ||
                       // if the name contains 'applause', 'banter, 'chat' 'encore' or 'talk' and it's over 90 seconds
                       ((track.Duration.Seconds > 90 ) &&
                        (( track.Name.ToLower().Contains( "applause" )) ||
                         ( track.Name.ToLower().Contains( "banter" )) ||
                         ( track.Name.ToLower().Contains( "chat" )) ||
                         ( track.Name.ToLower().Contains( "encore" )) ||
                         ( track.Name.ToLower().Contains( "talk" ))))) {
                        context.SelectedTracks.Remove( track );
                    }
                });
        }
    }
}
