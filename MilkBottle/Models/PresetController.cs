using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Caliburn.Micro;
using MilkBottle.Dto;
using MilkBottle.Interfaces;

namespace MilkBottle.Models {
    class PresetController : IPresetController, IHandle<Events.MilkInitialized> {
        private readonly IEventAggregator           mEventAggregator;
        private readonly ProjectMWrapper            mProjectM;
        private readonly Subject<MilkDropPreset>    mCurrentPreset;

        public  IObservable<MilkDropPreset>         CurrentPreset => mCurrentPreset.AsObservable();

        public PresetController( ProjectMWrapper projectM, IEventAggregator eventAggregator ) {
            mProjectM = projectM;
            mEventAggregator = eventAggregator;

            mCurrentPreset = new Subject<MilkDropPreset>();

            mEventAggregator.Subscribe( this );

            if( mProjectM.isInitialized()) {
                SubscribeCallbacks();
            }
        }

        public void Handle( Events.MilkInitialized args ) {
            SubscribeCallbacks();
        }

        private void SubscribeCallbacks() {
            mProjectM.setPresetCallback( OnPresetSwitched );
        }

        private void OnPresetSwitched(bool isHardCut, ulong presetIndex ) {
            mCurrentPreset.OnNext( new MilkDropPreset( mProjectM.getPresetName( presetIndex ), mProjectM.getPresetURL( presetIndex )));
        }

        public void Dispose() {
            mCurrentPreset?.Dispose();

            mEventAggregator.Unsubscribe( this );
        }
    }
}
