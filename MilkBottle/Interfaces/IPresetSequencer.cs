namespace MilkBottle.Interfaces {
    enum PresetSequence {
        Random,
        Sequential
    }

    interface IPresetSequencer {
        uint    SelectNextPreset( uint maxValue, uint currentIndex );
    }
}
