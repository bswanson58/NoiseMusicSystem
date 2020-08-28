using System;

namespace MilkBottle.Interfaces {
    public interface ILightPipePump : IDisposable {
        void    Initialize();
        void    EnableLightPipe( bool state );
        void    SetCaptureFrequency( int milliseconds );
    }
}
