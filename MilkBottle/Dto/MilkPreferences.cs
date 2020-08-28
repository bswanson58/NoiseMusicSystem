using System;
using MilkBottle.Models;

namespace MilkBottle.Dto {
    class MilkPreferences {
        public  int     PresetPlayDurationInSeconds {  get; set; }
        public  bool    PlayPresetsRandomly { get; set; }
        public  bool    BlendPresetTransition { get; set; }
        public  string  CurrentPresetLibrary { get; set; }
        public  bool    ShouldMinimizeToTray { get; set; }
        public  bool    DisplayControllerWhenMaximized { get; set; }
        public  bool    DisplayFps { get; set; }
        public  string  DefaultScene { get; set; }
        public  double  Latitude { get; set; }
        public  double  Longitude { get; set; }
        public  string  CurrentMood { get; set; }
        public  int     SceneRatingsBoostMode { get; set; }
        public  bool    LightPipeEnabled { get; set; }

        public MilkPreferences() {
            CurrentPresetLibrary = String.Empty;
            CurrentMood = String.Empty;

            PresetPlayDurationInSeconds = 7;
            PlayPresetsRandomly = true;
            BlendPresetTransition = true;

            ShouldMinimizeToTray = false;
            DisplayControllerWhenMaximized = true;
            DisplayFps = false;

            DefaultScene = String.Empty;

            Latitude = 41.997;
            Longitude = -88.458;

            SceneRatingsBoostMode = RatingsBoostMode.PreferMusicOverMood;

            LightPipeEnabled = false;
        }
    }
}
