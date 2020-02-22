using System;

namespace MilkBottle.Dto {
    class MilkPreferences {
        public  int     PresetPlayDurationInSeconds {  get; set; }
        public  bool    PlayPresetsRandomly { get; set; }
        public  bool    BlendPresetTransition { get; set; }
        public  string  CurrentPresetLibrary { get; set; }
        public  bool    ShouldMinimizeToTray { get; set; }
        public  bool    DisplayControllerWhenMaximized { get; set; }
        public  bool    DisplayFps { get; set; }

        public MilkPreferences() {
            CurrentPresetLibrary = String.Empty;

            PresetPlayDurationInSeconds = 7;
            PlayPresetsRandomly = true;
            BlendPresetTransition = true;

            ShouldMinimizeToTray = false;
            DisplayControllerWhenMaximized = true;
            DisplayFps = false;
        }
    }
}
