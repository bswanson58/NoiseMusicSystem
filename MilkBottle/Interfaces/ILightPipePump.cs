using System;

namespace MilkBottle.Interfaces {
    public interface ILightPipePump : IDisposable {
        bool    IsEnabled { get; }
        void    Initialize();
        void    EnableLightPipe( bool state, bool startLightPipeIfDesired = false );
        void    SetCaptureFrequency( int milliseconds );
    }
}
