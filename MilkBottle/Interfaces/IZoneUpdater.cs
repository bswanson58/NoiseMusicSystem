using System;
using System.Threading.Tasks;
using LightPipe.Dto;
using MilkBottle.Dto;

namespace MilkBottle.Interfaces {
    interface IZoneUpdater {
        int                         CaptureFrequency { get; set; }
        int                         ZoneColorsLimit { get; set; }
        double                      OverallLightBrightness { get; set; }

        Task<bool>                  Start();
        void                        Stop();
        bool                        IsRunning { get; }

        void                        UpdateZone( ZoneSummary zone );

        IObservable<ZoneBulbState>  BulbStates { get; }
    }
}
