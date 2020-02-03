using Noise.Infrastructure.Dto;

namespace Noise.UI.Dto {
    public struct CombinedPlayStrategy {
        public ePlayAdjacentStrategy    PlayStrategy { get; }
        public bool                     DoNotPlay { get; }

        public CombinedPlayStrategy( ePlayAdjacentStrategy strategy, bool doNotPlay ) {
            PlayStrategy = strategy;
            DoNotPlay = doNotPlay;
        }
    }
}
