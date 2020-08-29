using MilkBottle.Interfaces;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class LightPipeControlViewModel : PropertyChangeBase {
        private readonly ILightPipePump     mLightPipePump;

        public LightPipeControlViewModel( ILightPipePump pump ) {
            mLightPipePump = pump;
        }
    }
}
