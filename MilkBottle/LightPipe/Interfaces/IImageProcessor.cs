using System;
using System.Drawing;
using LightPipe.Dto;

namespace LightPipe.Interfaces {
    public interface IImageProcessor : IDisposable {
        void                        ProcessImage( Bitmap image );

        long                        ElapsedTime { get; }

        IObservable<ZoneSummary>    ZoneUpdate { get; }
    }
}
