using Noise.Infrastructure.Dto;

namespace Noise.Core.PlayStrategies.Exhausted {
    abstract class ExhaustedHandlerBase : IExhaustedPlayHandler {
        public string HandlerEnum { get; }

        protected ExhaustedHandlerBase( eTrackPlaySuggesters handlerId ) {
            HandlerEnum = handlerId.ToString();
        }

        protected ExhaustedHandlerBase( eTrackPlayDisqualifiers handlerId ) {
            HandlerEnum = handlerId.ToString();
        }

        public abstract void SelectTrack( IExhaustedSelectionContext context );
    }
}
