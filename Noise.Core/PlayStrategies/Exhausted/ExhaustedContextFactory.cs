using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies.Exhausted {
    class ExhaustedContextFactory : IExhaustedContextFactory {
        public IExhaustedSelectionContext CreateContext( IPlayQueue playQueue, long suggesterParameter ) {
            return new ExhaustedSelectionContext( playQueue, suggesterParameter );
        }
    }
}
