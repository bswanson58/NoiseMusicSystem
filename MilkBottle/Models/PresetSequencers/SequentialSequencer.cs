using MilkBottle.Interfaces;

namespace MilkBottle.Models.PresetSequencers {
    class SequentialSequencer : IPresetSequencer {
        public uint SelectNextPreset( uint maxValue, uint currentIndex ) {
            var retValue = currentIndex + 1;

            if( retValue >= maxValue ) {
                retValue = 0;
            }

            return retValue;
        }
    }
}
