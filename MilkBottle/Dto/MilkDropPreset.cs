namespace MilkBottle.Dto {
    class MilkDropPreset {
        public  string  PresetName { get; }
        public  string  PresetLocation { get; }

        public MilkDropPreset( string name, string location ) {
            PresetName = name;
            PresetLocation = location;
        }
    }
}
