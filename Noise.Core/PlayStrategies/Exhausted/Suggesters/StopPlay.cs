using Noise.Infrastructure.Dto;

namespace Noise.Core.PlayStrategies.Exhausted.Suggesters {
    class StopPlay : ExhaustedHandlerBase {
        public StopPlay() :
            base( eTrackPlaySuggesters.Stop ) { }

        public override void SelectTrack( IExhaustedSelectionContext context ) { }
    }
}
