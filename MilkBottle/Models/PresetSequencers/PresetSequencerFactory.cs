using MilkBottle.Interfaces;

namespace MilkBottle.Models.PresetSequencers {
    class PresetSequencerFactory : IPresetSequencerFactory {
        private readonly SequentialSequencer    mSequentialSequencer;
        private readonly RandomSequencer        mRandomSequencer;

        public PresetSequencerFactory( SequentialSequencer sequentialSequencer, RandomSequencer randomSequencer ) {
            mSequentialSequencer = sequentialSequencer;
            mRandomSequencer = randomSequencer;
        }

        public IPresetSequencer CreateSequencer( PresetSequence forSequence ) {
            var retValue = default( IPresetSequencer );

            switch( forSequence ) {
                case PresetSequence.Random:
                    retValue = mRandomSequencer;
                    break;
                
                case PresetSequence.Sequential:
                    retValue = mSequentialSequencer;
                    break;
            }

            return retValue;
        }
    }
}
