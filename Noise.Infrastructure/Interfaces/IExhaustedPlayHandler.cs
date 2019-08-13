using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
    public interface IStrategyDescription {
        eTrackPlayHandlers  Identifier { get; }
        eTrackPlayStrategy  StrategyType { get; }
        string              Name { get; }
        string              Description { get; }

        bool                RequiresParameters { get; }
    }

    public interface IExhaustedPlayHandler : IStrategyDescription {
        void    SelectTrack( IExhaustedSelectionContext context );
    }
}
