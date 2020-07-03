using System;
using System.Windows;
using OpenTK;

namespace MilkBottle.Interfaces {
    interface IMilkController : IDisposable {
        void    Initialize( GLControl glControl );
        void    MilkConfigurationUpdated();

        void    OnSizeChanged( Size size );

        void    StartVisualization();
        void    StopVisualization();

        bool    IsRunning { get; }
    }
}
