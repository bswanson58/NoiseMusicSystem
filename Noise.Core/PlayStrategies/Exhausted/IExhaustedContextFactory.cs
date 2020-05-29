using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies.Exhausted {
    public interface IExhaustedContextFactory {
        IExhaustedSelectionContext  CreateContext( IPlayQueue playQueue, long suggesterParameter );
    }
}
