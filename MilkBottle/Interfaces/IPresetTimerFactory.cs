namespace MilkBottle.Interfaces {
    interface IPresetTimerFactory {
        IPresetTimer    CreateTimer( PresetTimer ofType );
    }
}
