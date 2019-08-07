namespace Noise.Core.PlayStrategies.Exhausted {
    interface IExhaustedPlayHandler {
        string  HandlerEnum { get; }

        void    SelectTrack( IExhaustedSelectionContext context );
    }
}
