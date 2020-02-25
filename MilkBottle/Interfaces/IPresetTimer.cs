using System;

namespace MilkBottle.Interfaces {
    enum PresetTimer {
        FixedDuration,
        Infinite
    }

    interface IPresetTimer {
        void    StartTimer();
        void    StopTimer();

        void    ReloadTimer();
        void    SetDuration( int seconds );

        event   EventHandler PresetTimeElapsed;
    }
}
