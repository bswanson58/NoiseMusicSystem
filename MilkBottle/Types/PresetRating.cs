using MilkBottle.Support;

namespace MilkBottle.Types {
    class PresetRating {
        public  static int      DoNotPlayValue => -1;
        public  static int      UnRatedValue = 0;
        public  static int      MinimumValue => -1;
        public  static int      MaximumValue => 5;
        public  int             Value { get; }

        private PresetRating( int value ) {
            Value = value.Clamp( MinimumValue, MaximumValue );
        }

        public static PresetRating Create( int value ) {
            return new PresetRating( value );
        }

        public static implicit operator int ( PresetRating presetDuration ) {
            return presetDuration.Value;
        }

        public static implicit operator PresetRating( int mutable ) {
            return new PresetRating( mutable );
        }
    }
}
