﻿using System;
using MilkBottle.Dto;

namespace MilkBottle.Interfaces {
    interface IPresetController : IDisposable {
        IObservable<MilkDropPreset>     CurrentPreset { get; }

        void    LoadLibrary( string libraryName );

        void    SelectNextPreset();
        void    SelectPreviousPreset();

        string  CurrentPresetLibrary { get; }
        bool    BlendPresetTransition {  get; set; }
        int     PresetDuration { get; set; }
        bool    PresetCycling { get; set; }
    }
}
