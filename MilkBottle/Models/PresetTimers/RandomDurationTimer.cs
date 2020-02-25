using System;
using System.Windows.Threading;
using MilkBottle.Interfaces;

namespace MilkBottle.Models.PresetTimers {
    class RandomDurationTimer : IPresetTimer {
        private readonly DispatcherTimer    mPresetTimer;
        private readonly Random             mRandom;
        private int                         mTimerSeconds;

        public event EventHandler PresetTimeElapsed = delegate { };

        public RandomDurationTimer() {
            mRandom = new Random( DateTime.Now.Millisecond );

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
            var randomTime = Math.Max( 1, mRandom.Next((int)( mTimerSeconds * 0.3 ), (int)( mTimerSeconds * 1.7 )));

            mPresetTimer.Interval = TimeSpan.FromSeconds( randomTime );
        }

        public void SetDuration( int seconds ) {
            mTimerSeconds = seconds;

            ReloadTimer();
        }
    }
}
