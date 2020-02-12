namespace MilkBottle.Dto {
    class MilkDropPreset {
        public  string  PresetName { get; set; }
        public  string  PresetLocation { get; set; }

        public MilkDropPreset( string name, string location ) {
            PresetName = name;
            PresetLocation = location;
        }
    }
}
