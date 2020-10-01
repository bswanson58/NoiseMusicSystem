using System.Threading.Tasks;
using LightPipe.Dto;

namespace MilkBottle.Interfaces {
    interface IZoneUpdater {
        int             CaptureFrequency { get; set; }
        int             ZoneColorsLimit { get; set; }
        double          OverallLightBrightness { get; set; }

        Task<bool>      Start();
        void            Stop();
        Task<bool>      CheckRunning();

        void            UpdateZone( ZoneSummary zone );
    }
}
