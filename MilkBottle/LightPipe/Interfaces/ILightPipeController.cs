using System;

namespace LightPipe.Interfaces {
    public interface ILightPipeController : IDisposable {
        void    Initialize();
        void    EnableLightPipe( bool state );
        void    SetCaptureFrequency( int milliseconds );
    }
}
