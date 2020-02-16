using System;
using OpenTK;

namespace MilkBottle.Interfaces {
    interface IMilkController : IDisposable {
        void    Initialize( GLControl glControl );
        void    OnSizeChanged( int width, int height );

        void    StartVisualization();
        void    StopVisualization();

        bool    IsRunning { get; }
    }
}
