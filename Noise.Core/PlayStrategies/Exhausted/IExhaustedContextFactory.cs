using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies.Exhausted {
    interface IExhaustedContextFactory {
        IExhaustedSelectionContext  CreateContext( IPlayQueue playQueue );
    }
}
