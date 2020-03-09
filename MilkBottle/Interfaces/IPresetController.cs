using System;
using MilkBottle.Entities;
using MilkBottle.Types;

namespace MilkBottle.Interfaces {
    interface IPresetController : IDisposable {
        IObservable<Preset>     CurrentPreset { get; }

        void            Initialize();
        void            MilkConfigurationUpdated();
        void            ConfigurePresetTimer( PresetTimer timerType );
        void            ConfigurePresetSequencer( PresetSequence forSequence );

        void            LoadLibrary( PresetLibrary library );

        void            StopPresetCycling();
        void            StartPresetCycling();

        void            SelectNextPreset();
        void            SelectPreviousPreset();

        void            PlayPreset( Preset preset );
        Preset          GetPlayingPreset();

        bool            IsInitialized { get; }
        bool            IsRunning { get; }

        string          CurrentPresetLibrary { get; }
        bool            BlendPresetTransition {  get; set; }
        PresetDuration  PresetDuration { get; set; }
    }
}
