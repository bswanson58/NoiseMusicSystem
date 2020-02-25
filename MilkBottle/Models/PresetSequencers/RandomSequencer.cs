using MilkBottle.Interfaces;
using MilkBottle.Support;

namespace MilkBottle.Models.PresetSequencers {
    class RandomSequencer : IPresetSequencer {
        private readonly LimitedRepeatingRandom     mRandom;

        public RandomSequencer() {
            mRandom = new LimitedRepeatingRandom( 0.6 );
        }

        public uint SelectNextPreset( uint maxValue, uint currentIndex ) {
            var retValue = (uint)mRandom.Next( 0, (int)maxValue );

            return retValue;
        }
    }
}
