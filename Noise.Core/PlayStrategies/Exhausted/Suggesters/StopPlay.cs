using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies.Exhausted.Suggesters {
    public class StopPlay : ExhaustedHandlerBase {
        public StopPlay() :
            base( eTrackPlayHandlers.Stop, eTrackPlayStrategy.Suggester, "Stop Playing", "play is stopped" ) { }

        public override void SelectTrack( IExhaustedSelectionContext context ) { }
    }
}
