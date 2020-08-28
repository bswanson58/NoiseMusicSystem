using System;

namespace LightPipe.Dto {
    public class LightPipeConfiguration {
        public  string      ZoneGroupId { get; set; }

        public LightPipeConfiguration() {
            ZoneGroupId = String.Empty;
        }
    }
}
