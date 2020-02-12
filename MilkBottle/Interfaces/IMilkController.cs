using System;
using MilkBottle.Dto;
using OpenTK;

namespace MilkBottle.Interfaces {
    interface IMilkController : IDisposable {
        void    Initialize( GLControl glControl );
        void    OnSizeChanged( int width, int height );

        void    StartVisualization();
        void    StopVisualization();

        IObservable<MilkDropPreset>     CurrentPreset { get; }
    }
}
