using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies.Exhausted {
    interface IExhaustedPlayHandler : IItemDescription {
        void    SelectTrack( IExhaustedSelectionContext context );
    }
}
