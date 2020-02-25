using System;
using MilkBottle.Interfaces;

namespace MilkBottle.Models.PresetTimers {
    class InfiniteDurationTimer : IPresetTimer {
        public event EventHandler PresetTimeElapsed;

        public void StartTimer() { }
        public void StopTimer() { }
        public void ReloadTimer() { }
        public void SetDuration( int seconds ) { }
    }
}
