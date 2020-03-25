using MilkBottle.Entities;
using ReusableBits.Platform;

namespace MilkBottle.Interfaces {
    interface ISyncManager {
        PresetScene     SelectScene( PlaybackEvent forEvent );
    }
}
