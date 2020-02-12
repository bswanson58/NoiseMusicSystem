using System;
using MilkBottle.Dto;

namespace MilkBottle.Interfaces {
    interface IPresetController : IDisposable {
        IObservable<MilkDropPreset>     CurrentPreset { get; }

        void    SelectNextPreset();
        void    SelectPreviousPreset();
        void    SelectRandomPreset();

        bool    PresetOverlap {  get; set; }
        bool    PresetCycling { get; set; }
    }
}
