using System;
using MilkBottle.Dto;

namespace MilkBottle.Interfaces {
    interface IPresetController : IDisposable {
        IObservable<MilkDropPreset>     CurrentPreset { get; }

        void    selectNextPreset();
        void    selectPreviousPreset();
        void    selectRandomPreset();

        void    setPresetOverlap( bool state );
        void    setPresetCycling( bool state );
    }
}
