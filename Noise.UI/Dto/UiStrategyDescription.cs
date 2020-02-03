using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.UI.Dto {
    public class UiStrategyDescription {
        private readonly IStrategyDescription   mStrategyDescription;

        public  string                          Name => mStrategyDescription.Name;
        public  string                          Description => mStrategyDescription.Description;
        public  eTrackPlayHandlers              StrategyId => mStrategyDescription.Identifier;

        public  bool                            IsSelected { get; set; }

        public UiStrategyDescription( IStrategyDescription description ) {
            mStrategyDescription = description;
        }
    }
}
