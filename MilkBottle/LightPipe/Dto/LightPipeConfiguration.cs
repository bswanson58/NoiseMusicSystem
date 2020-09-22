using System;

namespace LightPipe.Dto {
    public class LightPipeConfiguration {
        public  string      ZoneGroupId { get; set; }
        public  int         BlacknessLimit { get; set; }
        public  int         WhitenessLimit { get; set; }
        public  bool        BoostLuminosity { get; set; }
        public  bool        BoostSaturation { get; set; }

        public LightPipeConfiguration() {
            ZoneGroupId = String.Empty;

            BlacknessLimit = 25;
            WhitenessLimit = 90;
            BoostLuminosity = true;
            BoostSaturation = true;
        }
    }
}
