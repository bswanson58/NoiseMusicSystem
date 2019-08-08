using Noise.Infrastructure.Dto;

namespace Noise.Core.PlayStrategies.Exhausted.Suggesters {
    class StopPlay : IExhaustedPlayHandler {
        public string HandlerEnum => eTrackPlaySuggesters.Stop.ToString();

        public void SelectTrack( IExhaustedSelectionContext context ) { }
    }
}
