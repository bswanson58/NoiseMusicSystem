using System;
using System.Threading.Tasks;

namespace MilkBottle.Interfaces {
    public interface ILightPipePump : IDisposable {
        bool        IsEnabled { get; }
        Task<bool>  Initialize();
        Task<bool>  EnableLightPipe( bool state, bool startLightPipeIfDesired = false );

        void        SetCaptureFrequency( int milliseconds );
        double      OverallBrightness { get; set; }
    }
}
