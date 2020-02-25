using MilkBottle.Interfaces;

namespace MilkBottle.Models.PresetTimers {
    class PresetTimerFactory : IPresetTimerFactory {
        private readonly FixedDurationTimer     mFixedTimer;
        private readonly InfiniteDurationTimer  mInfiniteTimer;

        public PresetTimerFactory( FixedDurationTimer fixedTimer, InfiniteDurationTimer infiniteTimer ) {
            mFixedTimer = fixedTimer;
            mInfiniteTimer = infiniteTimer;
        }

        public IPresetTimer CreateTimer( PresetTimer ofType ) {
            var retValue = default( IPresetTimer );

            switch( ofType ) {
                case PresetTimer.FixedDuration:
                    retValue = mFixedTimer;
                    break;

                case PresetTimer.Infinite:
                    retValue = mInfiniteTimer;
                    break;
            }

            return retValue;
        }
    }
}
