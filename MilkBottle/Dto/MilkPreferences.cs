using System;

namespace MilkBottle.Dto {
    class MilkPreferences {
        public  String  InitialPresetFolder { get; set; }

        public MilkPreferences() {
            InitialPresetFolder = String.Empty;
        }
    }
}
