using System;

namespace MilkBottle.Dto {
    class MilkPreferences {
        public  String  InitialPresetFolder { get; set; }
        public  int     PresetPlayDurationInSeconds {  get; set; }
        public  bool    PlayPresetsRandomly { get; set; }
        public  bool    BlendPresetTransition { get; set; }
        public  string  CurrentPresetLibrary { get; set; }
        public  bool    ShouldMinimizeToTray { get; set; }

        public MilkPreferences() {
            InitialPresetFolder = String.Empty;
            CurrentPresetLibrary = String.Empty;

            PresetPlayDurationInSeconds = 7;
            PlayPresetsRandomly = true;
            BlendPresetTransition = true;

            ShouldMinimizeToTray = false;
        }
    }
}
