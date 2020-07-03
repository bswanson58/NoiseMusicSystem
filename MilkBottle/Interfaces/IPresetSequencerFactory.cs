namespace MilkBottle.Interfaces {
    interface IPresetSequencerFactory {
        IPresetSequencer    CreateSequencer( PresetSequence forSequence );
    }
}
