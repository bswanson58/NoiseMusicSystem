using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies.Exhausted {
    class ExhaustedContextFactory : IExhaustedContextFactory {
        public IExhaustedSelectionContext CreateContext( IPlayQueue playQueue ) {
            return new ExhaustedSelectionContext( playQueue );
        }
    }
}
