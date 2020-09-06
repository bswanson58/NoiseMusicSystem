using System;
using System.Threading.Tasks;

namespace MilkBottle.Interfaces {
    public interface ILightPipePump : IDisposable {
        bool        IsEnabled { get; }
        Task<bool>  Initialize();
        Task<bool>  EnableLightPipe( bool state, bool startLightPipeIfDesired = false );

        int         CaptureFrequency { get; set; }
        double      OverallBrightness { get; set; }
        int         BlacknessLimit { get; set; }
        int         WhitenessLimit { get; set; }
        int         ZoneColorsLimit { get; set; }
    }
}
