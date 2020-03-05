using System;
using MilkBottle.Dto;
using MilkBottle.Types;

namespace MilkBottle.Interfaces {
    interface IPresetController : IDisposable {
        IObservable<MilkDropPreset>     CurrentPreset { get; }

        void            Initialize();
        void            MilkConfigurationUpdated();
        void            ConfigurePresetTimer( PresetTimer timerType );
        void            ConfigurePresetSequencer( PresetSequence forSequence );

        void            LoadLibrary( string libraryName );

        void            StopPresetCycling();
        void            StartPresetCycling();

        void            SelectNextPreset();
        void            SelectPreviousPreset();

        void            PlayPreset( MilkDropPreset preset );
        MilkDropPreset  GetPlayingPreset();

        bool            IsInitialized { get; }
        bool            IsRunning { get; }

        string          CurrentPresetLibrary { get; }
        bool            BlendPresetTransition {  get; set; }
        PresetDuration  PresetDuration { get; set; }
    }
}
