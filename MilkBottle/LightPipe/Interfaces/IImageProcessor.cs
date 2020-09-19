﻿using System;
using System.Drawing;
using LightPipe.Dto;

namespace LightPipe.Interfaces {
    public interface IImageProcessor : IDisposable {
        void                        ProcessImage( Bitmap image );

        long                        ElapsedTime { get; }
        int                         BlacknessLimit { get; set; }
        int                         WhitenessLimit { get; set; }

        IObservable<ZoneSummary>    ZoneUpdate { get; }
    }
}