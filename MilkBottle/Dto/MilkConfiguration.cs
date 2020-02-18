using System;

namespace MilkBottle.Dto {
    class MilkConfiguration {
        public  int     MeshWidth { get; set; }
        public  int		MeshHeight { get; set; }
        public  int		SmoothPresetDuration { get; set; }
        public  float	BeatSensitivity { get; set; }
        public  bool	AspectCorrection { get; set; }
        public  bool	SoftCutRatingsEnabled { get; set; }

        public MilkConfiguration() {
            MeshWidth = 32;
            MeshHeight = 24;
            SmoothPresetDuration = 10;
            BeatSensitivity = 10.0f;
            AspectCorrection = true;
            SoftCutRatingsEnabled = false;
        }

        public void SetNativeConfiguration( ProjectMSettings settings ) {
            settings.MeshHeight = MeshHeight;
            settings.MeshWidth = MeshWidth;
            settings.SmoothPresetDuration = SmoothPresetDuration;
            settings.BeatSensitivity = BeatSensitivity;
            settings.AspectCorrection = AspectCorrection;
            settings.ShuffleEnabled = false;
            settings.SoftCutRatingsEnabled = SoftCutRatingsEnabled;
        }
    }
}
