using MilkBottle.Entities;

namespace MilkBottle.Interfaces {
    interface ISidecarHandler {
        void    SaveSidecar( Preset forPreset );
        Preset  LoadSidecar( Preset forPreset );
    }
}
