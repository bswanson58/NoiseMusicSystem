using System;
using System.Windows.Threading;
using MilkBottle.Interfaces;

namespace MilkBottle.Models.PresetTimers {
    class FixedDurationTimer : IPresetTimer {
        private readonly DispatcherTimer    mPresetTimer;
        private int                         mTimerSeconds;

        public event EventHandler PresetTimeElapsed = delegate { };

        public FixedDurationTimer() {
            mPresetTimer = new DispatcherTimer();
            mPresetTimer.Tick += OnPresetTimer;
        }

        private void OnPresetTimer( object sender, EventArgs args ) {
            PresetTimeElapsed.Invoke( this, EventArgs.Empty );
        }

        public void StartTimer() {
            ReloadTimer();

            mPresetTimer.Start();
        }

        public void StopTimer() {
            mPresetTimer.Stop();
        }

        public void ReloadTimer() {
            mPresetTimer.Interval = TimeSpan.FromSeconds( mTimerSeconds );
        }

        public void SetDuration( int seconds ) {
            mTimerSeconds = seconds;

            ReloadTimer();
        }
    }
}
