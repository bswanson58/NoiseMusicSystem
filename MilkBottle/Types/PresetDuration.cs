using MilkBottle.Support;

namespace MilkBottle.Types {
    class PresetDuration {
        public  static int      MinimumValue => 5;
        public  static int      MaximumValue => 60;
        public  int             Value { get; }

        private PresetDuration( int value ) {
            Value = value.Clamp( MinimumValue, MaximumValue );
        }

        public static PresetDuration Create( int value ) {
            return new PresetDuration( value );
        }

        public static implicit operator int ( PresetDuration presetDuration ) {
            return presetDuration.Value;
        }

        public static implicit operator PresetDuration( int mutable ) {
            return new PresetDuration( mutable );
        }
    }
}
