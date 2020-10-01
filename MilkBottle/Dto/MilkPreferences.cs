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
        public  bool    DisplayBuildDate { get; set; }
        public  string  DefaultScene { get; set; }
        public  double  Latitude { get; set; }
        public  double  Longitude { get; set; }
        public  string  CurrentMood { get; set; }
        public  int     SceneRatingsBoostMode { get; set; }
        public  int     LightPipeCaptureFrequency { get; set; }
        public  string  LightPipePairing { get; set; }
        public  int     ZoneColorsLimit { get; set; }
        public  double  OverallBrightness;

        public MilkPreferences() {
            CurrentPresetLibrary = String.Empty;
            CurrentMood = String.Empty;

            PresetPlayDurationInSeconds = 7;
            PlayPresetsRandomly = true;
            BlendPresetTransition = true;

            ShouldMinimizeToTray = false;
            DisplayControllerWhenMaximized = true;
            DisplayFps = false;
            DisplayBuildDate = false;

            DefaultScene = String.Empty;

            Latitude = 41.997;
            Longitude = -88.458;

            SceneRatingsBoostMode = RatingsBoostMode.PreferMusicOverMood;

            LightPipeCaptureFrequency = 100;
            LightPipePairing = String.Empty;
            ZoneColorsLimit = 10;
            OverallBrightness = 0.8;
        }
    }
}
